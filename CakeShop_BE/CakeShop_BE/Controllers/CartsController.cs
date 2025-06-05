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

namespace CakeShop_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CartsController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public CartsController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpPost("AddToCart")]
		public JsonResult AddToCart([FromBody] Cart cartItem)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			if (cartItem.UserID <= 0 || cartItem.ProductID <= 0 || cartItem.Quantity <= 0)
			{
				return new JsonResult(new { status = "error", message = "Thông tin không hợp lệ." });
			}

			try
			{
				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();
					SqlTransaction transaction = myCon.BeginTransaction();

					try
					{
						//  Lấy số lượng sản phẩm có sẵn trong kho
						string checkStockQuery = "SELECT Quantity FROM Products WHERE ProductID = @ProductID";
						int stockQuantity = 0;

						using (SqlCommand cmd = new SqlCommand(checkStockQuery, myCon, transaction))
						{
							cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = cartItem.ProductID;
							stockQuantity = Convert.ToInt32(cmd.ExecuteScalar());
						}

						//  Lấy số lượng sản phẩm đã có trong giỏ hàng của người dùng
						string checkCartQuery = "SELECT Quantity FROM Cart WHERE UserID = @UserID AND ProductID = @ProductID";
						int currentQuantity = 0;

						using (SqlCommand cmd = new SqlCommand(checkCartQuery, myCon, transaction))
						{
							cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = cartItem.UserID;
							cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = cartItem.ProductID;
							var result = cmd.ExecuteScalar();
							if (result != null)
							{
								currentQuantity = Convert.ToInt32(result);
							}
						}

						// Kiểm tra tổng số lượng sau khi thêm vào giỏ hàng
						if (currentQuantity + cartItem.Quantity > stockQuantity)
						{
							return new JsonResult(new { status = "error", message = "Số lượng trong kho không đủ." });
						}

						// Nếu sản phẩm đã có trong giỏ hàng, cập nhật số lượng
						if (currentQuantity > 0)
						{
							string updateCartQuery = "UPDATE Cart SET Quantity = Quantity + @Quantity WHERE UserID = @UserID AND ProductID = @ProductID";

							using (SqlCommand cmd = new SqlCommand(updateCartQuery, myCon, transaction))
							{
								cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = cartItem.Quantity;
								cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = cartItem.UserID;
								cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = cartItem.ProductID;
								cmd.ExecuteNonQuery();
							}
						}
						else
						{
							//  Nếu sản phẩm chưa có trong giỏ hàng, thêm mới
							string insertCartQuery = "INSERT INTO Cart (UserID, ProductID, Quantity) VALUES (@UserID, @ProductID, @Quantity)";

							using (SqlCommand cmd = new SqlCommand(insertCartQuery, myCon, transaction))
							{
								cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = cartItem.UserID;
								cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = cartItem.ProductID;
								cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = cartItem.Quantity;
								cmd.ExecuteNonQuery();
							}
						}

						// Commit giao dịch
						transaction.Commit();
						return new JsonResult(new { status = "success", message = "Đã thêm sản phẩm vào giỏ hàng." });
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						return new JsonResult(new { status = "error", message = $"Lỗi: {ex.Message}" });
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonResult(new { status = "error", message = $"Lỗi kết nối DB: {ex.Message}" });
			}
		}


		[HttpGet("GetCart/{userId}")]
		public JsonResult GetCart(int userId)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			try
			{
				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();

					string cartQuery = @"
                SELECT 
                    c.CartID, c.ProductID, c.Quantity, 
                    p.ProductName, p.Price, p.ImageURL, 
                    (c.Quantity * p.Price) AS TotalAmount
                FROM Cart c
                JOIN Products p ON c.ProductID = p.ProductID
                WHERE c.UserID = @UserID";

					var cartItems = new List<object>();

					using (SqlCommand cmd = new SqlCommand(cartQuery, myCon))
					{
						cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								cartItems.Add(new
								{
									CartID = reader["CartID"],
									ProductID = reader["ProductID"],
									ProductName = reader["ProductName"],
									Price = reader["Price"],
									ImageURL = reader["ImageURL"],
									Quantity = reader["Quantity"],
									TotalAmount = reader["TotalAmount"]
								});
							}
						}
					}

					if (cartItems.Count == 0)
					{
						return new JsonResult(new { status = "success", message = "Giỏ hàng trống." });
					}

					return new JsonResult(new { status = "success", data = cartItems });
				}
			}
			catch (Exception ex)
			{
				return new JsonResult(new { status = "error", message = $"Lỗi kết nối DB: {ex.Message}" });
			}
		}

		[HttpPut("UpdateCart")]
		public JsonResult UpdateCart([FromBody] Cart cartItem)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			if (cartItem.UserID <= 0 || cartItem.ProductID <= 0 || cartItem.Quantity < 0)
			{
				return new JsonResult(new { status = "error", message = "Thông tin không hợp lệ." });
			}

			try
			{
				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();
					SqlTransaction transaction = myCon.BeginTransaction();

					try
					{
						// Kiểm tra số lượng sản phẩm có sẵn trong kho
						string checkStockQuery = "SELECT Quantity FROM Products WHERE ProductID = @ProductID";
						int stockQuantity = 0;

						using (SqlCommand cmd = new SqlCommand(checkStockQuery, myCon, transaction))
						{
							cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = cartItem.ProductID;
							stockQuantity = Convert.ToInt32(cmd.ExecuteScalar());
						}

						// Kiểm tra sản phẩm có tồn tại trong giỏ hàng không
						string checkCartQuery = "SELECT Quantity FROM Cart WHERE UserID = @UserID AND ProductID = @ProductID";
						int currentQuantity = 0;

						using (SqlCommand cmd = new SqlCommand(checkCartQuery, myCon, transaction))
						{
							cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = cartItem.UserID;
							cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = cartItem.ProductID;
							var result = cmd.ExecuteScalar();
							if (result != null)
							{
								currentQuantity = Convert.ToInt32(result);
							}
							else
							{
								return new JsonResult(new { status = "error", message = "Sản phẩm không tồn tại trong giỏ hàng." });
							}
						}

						// Kiểm tra nếu số lượng cập nhật vượt quá số lượng trong kho
						if (cartItem.Quantity > stockQuantity)
						{
							return new JsonResult(new { status = "error", message = "Số lượng trong kho không đủ." });
						}

						// Nếu số lượng cập nhật là 0, xóa sản phẩm khỏi giỏ hàng
						if (cartItem.Quantity == 0)
						{
							string deleteCartQuery = "DELETE FROM Cart WHERE UserID = @UserID AND ProductID = @ProductID";
							using (SqlCommand cmd = new SqlCommand(deleteCartQuery, myCon, transaction))
							{
								cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = cartItem.UserID;
								cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = cartItem.ProductID;
								cmd.ExecuteNonQuery();
							}
						}
						else
						{
							// Cập nhật số lượng sản phẩm trong giỏ hàng
							string updateCartQuery = "UPDATE Cart SET Quantity = @Quantity WHERE UserID = @UserID AND ProductID = @ProductID";
							using (SqlCommand cmd = new SqlCommand(updateCartQuery, myCon, transaction))
							{
								cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = cartItem.Quantity;
								cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = cartItem.UserID;
								cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = cartItem.ProductID;
								cmd.ExecuteNonQuery();
							}
						}

						transaction.Commit();
						return new JsonResult(new { status = "success", message = "Cập nhật giỏ hàng thành công." });
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						return new JsonResult(new { status = "error", message = $"Lỗi: {ex.Message}" });
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonResult(new { status = "error", message = $"Lỗi kết nối DB: {ex.Message}" });
			}
		}

		[HttpDelete("DeleteFromCart/{userId}/{productId}")]
		public JsonResult DeleteFromCart(int userId, int productId)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			if (userId <= 0 || productId <= 0)
			{
				return new JsonResult(new { status = "error", message = "Thông tin không hợp lệ." });
			}

			try
			{
				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();
					SqlTransaction transaction = myCon.BeginTransaction();

					try
					{
						// Kiểm tra sản phẩm có tồn tại trong giỏ hàng không
						string checkCartQuery = "SELECT COUNT(*) FROM Cart WHERE UserID = @UserID AND ProductID = @ProductID";
						int cartItemExists = 0;

						using (SqlCommand cmd = new SqlCommand(checkCartQuery, myCon, transaction))
						{
							cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
							cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = productId;
							cartItemExists = Convert.ToInt32(cmd.ExecuteScalar());
						}

						if (cartItemExists == 0)
						{
							return new JsonResult(new { status = "error", message = "Sản phẩm không tồn tại trong giỏ hàng." });
						}

						// Xóa sản phẩm khỏi giỏ hàng
						string deleteCartQuery = "DELETE FROM Cart WHERE UserID = @UserID AND ProductID = @ProductID";
						using (SqlCommand cmd = new SqlCommand(deleteCartQuery, myCon, transaction))
						{
							cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
							cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = productId;
							cmd.ExecuteNonQuery();
						}

						transaction.Commit();
						return new JsonResult(new { status = "success", message = "Đã xóa sản phẩm khỏi giỏ hàng." });
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						return new JsonResult(new { status = "error", message = $"Lỗi: {ex.Message}" });
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonResult(new { status = "error", message = $"Lỗi kết nối DB: {ex.Message}" });
			}
		}

	}
}
