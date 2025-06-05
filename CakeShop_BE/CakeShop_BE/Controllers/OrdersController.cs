using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CakeShop_BE.Data;
using CakeShop_BE.Model;
using Microsoft.Data.SqlClient;
using System.Data;
using Azure.Core;

namespace CakeShop_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public OrdersController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpPost("CreateOrder/{userId}")]
		public async Task<IActionResult> CreateOrder(int userId, [FromBody] Order order)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			try
			{
				using var con = new SqlConnection(sqlDataSource);
				await con.OpenAsync();
				using var transaction = con.BeginTransaction();

				try
				{
					// 1. Lấy danh sách sản phẩm trong giỏ hàng của người dùng
					var cartItems = new List<(int ProductID, int Quantity, decimal Price)>();
					string getCartQuery = @"SELECT c.ProductID, c.Quantity, p.Price
                                    FROM Cart c
                                    JOIN Products p ON c.ProductID = p.ProductID
                                    WHERE c.UserID = @UserID";

					using (var cmd = new SqlCommand(getCartQuery, con, transaction))
					{
						cmd.Parameters.AddWithValue("@UserID", userId);
						using var reader = await cmd.ExecuteReaderAsync();

						while (await reader.ReadAsync())
						{
							cartItems.Add((
								Convert.ToInt32(reader["ProductID"]),
								Convert.ToInt32(reader["Quantity"]),
								Convert.ToDecimal(reader["Price"])
							));
						}
					}


					if (!cartItems.Any())
						return BadRequest(new { status = "error", message = "Giỏ hàng trống." });

					// 2. Tạo đơn hàng mới trong bảng Orders
					int orderId;
					string insertOrderQuery = @"INSERT INTO Orders (UserID, OrderDate, Status, FullName, Phone, Address, PaymentMethod, TotalAmount, PaymentProofImage)
                                        OUTPUT INSERTED.OrderID
                                        VALUES (@UserID, GETDATE(), @Status, @FullName, @Phone, @Address, @PaymentMethod, @TotalAmount, @PaymentProofImage)";

					using (var cmd = new SqlCommand(insertOrderQuery, con, transaction))
					{
						// Thêm các tham số cho câu lệnh SQL
						cmd.Parameters.AddWithValue("@UserID", userId);
						cmd.Parameters.AddWithValue("@Status", order.Status ?? "Pending"); 
						cmd.Parameters.AddWithValue("@FullName", order.FullName ?? "");
						cmd.Parameters.AddWithValue("@Phone", order.Phone ?? "");
						cmd.Parameters.AddWithValue("@Address", order.Address ?? "");
						cmd.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod ?? "");
						cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
						cmd.Parameters.AddWithValue("@PaymentProofImage", (object)order.PaymentProofImage ?? DBNull.Value);

						orderId = (int)await cmd.ExecuteScalarAsync();
					}

					// 3. Thêm chi tiết đơn hàng vào bảng OrderDetails
					foreach (var item in cartItems)
					{
						string insertDetailQuery = @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, Price)
                                             VALUES (@OrderID, @ProductID, @Quantity, @Price)";
						using var cmd = new SqlCommand(insertDetailQuery, con, transaction);
						cmd.Parameters.AddWithValue("@OrderID", orderId);
						cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
						cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
						cmd.Parameters.AddWithValue("@Price", item.Price);
						await cmd.ExecuteNonQueryAsync();
					}

					// 4. Xóa tất cả sản phẩm trong giỏ hàng sau khi đặt hàng thành công
					string clearCartQuery = "DELETE FROM Cart WHERE UserID = @UserID";
					using var clearCmd = new SqlCommand(clearCartQuery, con, transaction);
					clearCmd.Parameters.AddWithValue("@UserID", userId);
					await clearCmd.ExecuteNonQueryAsync();

					// 5. Cập nhật số lượng tồn kho của từng sản phẩm
					foreach (var item in cartItems)
					{
	
						string getQuantityQuery = @"SELECT Quantity FROM Products WHERE ProductID = @ProductID";

						int currentQuantity;
						using (var getCmd = new SqlCommand(getQuantityQuery, con, transaction))
						{
							getCmd.Parameters.AddWithValue("@ProductID", item.ProductID);
							var result = await getCmd.ExecuteScalarAsync();
							currentQuantity = result != null ? Convert.ToInt32(result) : 0;
						}


						int updatedQuantity = currentQuantity - item.Quantity;
						if (updatedQuantity < 0) updatedQuantity = 0; 

	
						string updateQuantityQuery = @"UPDATE Products SET Quantity = @Quantity WHERE ProductID = @ProductID";

						using (var updateCmd = new SqlCommand(updateQuantityQuery, con, transaction))
						{
							updateCmd.Parameters.AddWithValue("@ProductID", item.ProductID);
							updateCmd.Parameters.AddWithValue("@Quantity", updatedQuantity);
							await updateCmd.ExecuteNonQueryAsync();
						}
					}

					await transaction.CommitAsync();
					return Ok(new { status = "success", message = "Đặt hàng thành công.", OrderID = orderId });
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					return StatusCode(500, new { status = "error", message = $"Lỗi xử lý đơn hàng: {ex.Message}" });
				}
			}
			catch (Exception ex) { 

				return StatusCode(500, new { status = "error", message = $"Lỗi DB: {ex.Message}" });
			}
		}

		[HttpPost("UploadPaymentProof")]
		public async Task<IActionResult> UploadPaymentProof(IFormFile file)
		{
			try
			{
				// Kiểm tra xem có file được upload hay không
				if (file == null || file.Length == 0)
					return BadRequest(new { status = "error", message = "Không có file được chọn." });

				// Kiểm tra định dạng file - chỉ chấp nhận file ảnh
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
				var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

				if (!allowedExtensions.Contains(fileExtension))
					return BadRequest(new { status = "error", message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)." });

				// Kiểm tra kích thước file - tối đa 5MB
				if (file.Length > 5 * 1024 * 1024)
					return BadRequest(new { status = "error", message = "File không được vượt quá 5MB." });

				// Tạo thư mục lưu trữ nếu chưa tồn tại
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "payment-proofs");
				if (!Directory.Exists(uploadsFolder))
					Directory.CreateDirectory(uploadsFolder);

				// Tạo tên file duy nhất để tránh trùng lặp
				var fileName = $"{Guid.NewGuid()}{fileExtension}";
				var filePath = Path.Combine(uploadsFolder, fileName);

				// Lưu file vào thư mục
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				// Tạo URL để truy cập file
				var fileUrl = $"/uploads/payment-proofs/{fileName}";

				return Ok(new
				{
					status = "success",
					message = "Upload thành công.",
					fileUrl = fileUrl,
					fileName = fileName
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = "error", message = $"Lỗi upload file: {ex.Message}" });
			}
		}

		[HttpGet("UserOrders/List/{userId}")]
		public async Task<IActionResult> GetUserOrderList(int userId)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");
			var orders = new List<object>();

			try
			{
				using var con = new SqlConnection(sqlDataSource);
				await con.OpenAsync();

				string query = @"
            SELECT OrderID, OrderDate, Status, TotalAmount, PaymentMethod 
            FROM Orders 
            WHERE UserID = @UserID 
            ORDER BY OrderDate DESC";

				using var cmd = new SqlCommand(query, con);
				cmd.Parameters.AddWithValue("@UserID", userId);

				using var reader = await cmd.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					orders.Add(new
					{
						OrderID = reader["OrderID"],
						OrderDate = reader["OrderDate"],
						Status = reader["Status"],
						TotalAmount = reader["TotalAmount"],
						PaymentMethod = reader["PaymentMethod"]
					});
				}

				return Ok(new { status = "success", data = orders });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = "error", message = ex.Message });
			}
		}

		[HttpGet("OrderDetails/{orderId}")]
		public async Task<IActionResult> GetOrderDetails(int orderId)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");
			var orderDetails = new List<object>();
			object orderInfo = null;

			try
			{
				using var con = new SqlConnection(sqlDataSource);
				await con.OpenAsync();

				string query = @"
            SELECT 
                o.OrderID, 
                o.OrderDate, 
                o.Status, 
				o.FullName,
				o.Phone,
				o.Address,
                od.ProductID, 
                p.ProductName, 
                od.Quantity, 
                od.Price,
                (od.Quantity * od.Price) AS TotalAmount, 
                p.ImageURL
            FROM OrderDetails od
            JOIN Products p ON od.ProductID = p.ProductID
            JOIN Orders o ON od.OrderID = o.OrderID
            WHERE od.OrderID = @OrderID";

				using var cmd = new SqlCommand(query, con);
				cmd.Parameters.AddWithValue("@OrderID", orderId);

				using var reader = await cmd.ExecuteReaderAsync();

				while (await reader.ReadAsync())
				{
					// Lấy orderInfo từ bản ghi đầu tiên
					if (orderInfo == null)
					{
						orderInfo = new
						{
							OrderID = reader["OrderID"],
							OrderDate = reader["OrderDate"],
							Status = reader["Status"],
							FullName = reader["FullName"],
							Phone = reader["Phone"],
							Address = reader["Address"],
						};
					}

					orderDetails.Add(new
					{
						ProductID = reader["ProductID"],
						ProductName = reader["ProductName"],
						Quantity = reader["Quantity"],
						Price = reader["Price"],
						TotalAmount = reader["TotalAmount"],
						ImageURL = reader["ImageURL"]
					});
				}

				return Ok(new { status = "success", data = new { orderInfo, orderDetails } });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = "error", message = ex.Message });
			}
		}



		[HttpGet("AdminOrders")]
		public async Task<IActionResult> GetAllOrders()
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");
			var orders = new List<object>();

			try
			{
				using var con = new SqlConnection(sqlDataSource);
				await con.OpenAsync();

				string query = @"
                    SELECT o.OrderID, o.OrderDate, o.UserID, o.Status,
                           od.ProductID, p.ProductName, od.Quantity, od.Price,
                           (od.Quantity * od.Price) AS TotalAmount
                    FROM Orders o
                    JOIN OrderDetails od ON o.OrderID = od.OrderID
                    JOIN Products p ON od.ProductID = p.ProductID
                    ORDER BY o.OrderDate DESC";

				using var cmd = new SqlCommand(query, con);
				using var reader = await cmd.ExecuteReaderAsync();

				while (await reader.ReadAsync())
				{
					orders.Add(new
					{
						OrderID = reader["OrderID"],
						OrderDate = reader["OrderDate"],
						UserID = reader["UserID"],
						ProductName = reader["ProductName"],
						Quantity = reader["Quantity"],
						Price = reader["Price"],
						TotalAmount = reader["TotalAmount"],
						Status = reader["Status"]
					});
				}

				return Ok(new { status = "success", data = orders });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = "error", message = ex.Message });
			}
		}


			[HttpPut("UpdateStatus")]
		public async Task<IActionResult> UpdateOrderStatus([FromBody] dynamic body)
		{
			try
			{
				int orderId = body.OrderID;
				string status = body.Status;

				string sqlDataSource = _configuration.GetConnectionString("CakeShop");

				using var con = new SqlConnection(sqlDataSource);
				await con.OpenAsync();

				string query = "UPDATE Orders SET Status = @Status WHERE OrderID = @OrderID";

				using var cmd = new SqlCommand(query, con);
				cmd.Parameters.AddWithValue("@Status", status);
				cmd.Parameters.AddWithValue("@OrderID", orderId);

				int rows = await cmd.ExecuteNonQueryAsync();

				if (rows > 0)
					return Ok(new { status = "success", message = "Cập nhật trạng thái thành công." });

				return NotFound(new { status = "error", message = "Không tìm thấy đơn hàng." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { status = "error", message = ex.Message });
			}
		}

	}
}