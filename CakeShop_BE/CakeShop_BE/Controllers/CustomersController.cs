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
using CakeShop_BE.DTO;

namespace CakeShop_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CustomersController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public CustomersController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// Lấy danh sách tất cả người dùng
		[HttpGet]
		public IActionResult GetAllCustomers()
		{
			string query = "SELECT UserID, Username, Email, FullName, Phone, Address, RoleID FROM Users";
			DataTable table = new DataTable();
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					SqlDataReader myReader = myCommand.ExecuteReader();
					table.Load(myReader);
					myReader.Close();
					myCon.Close();
				}
			}
			return new JsonResult(table);
		}

		// Lấy thông tin chi tiết của một người dùng
		[HttpGet("{id}")]
		public IActionResult GetCustomersById(int id)
		{
			string query = "SELECT UserID, Username, Email, FullName, Phone, Address, RoleID FROM Users WHERE UserID = @UserId";
			DataTable table = new DataTable();
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					myCommand.Parameters.AddWithValue("@UserId", id);
					SqlDataReader myReader = myCommand.ExecuteReader();
					table.Load(myReader);
					myReader.Close();
					myCon.Close();
				}
			}
			return new JsonResult(table);
		}

		// Thêm người dùng mới
		[HttpPost]
		public IActionResult AddCustomer([FromBody] Customer customer)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();

				// Kiểm tra Username, Email, Phone đã tồn tại chưa
				string checkQuery = @"
            SELECT 
                (SELECT COUNT(1) FROM Users WHERE Username = @Username) AS UsernameExists,
                (SELECT COUNT(1) FROM Users WHERE Email = @Email) AS EmailExists,
                (SELECT COUNT(1) FROM Users WHERE Phone = @Phone) AS PhoneExists";

				using (SqlCommand checkCommand = new SqlCommand(checkQuery, myCon))
				{
					checkCommand.Parameters.AddWithValue("@Username", customer.Username);
					checkCommand.Parameters.AddWithValue("@Email", customer.Email);
					checkCommand.Parameters.AddWithValue("@Phone", customer.Phone);

					SqlDataReader reader = checkCommand.ExecuteReader();
					if (reader.Read())
					{
						bool usernameExists = reader.GetInt32(0) > 0;
						bool emailExists = reader.GetInt32(1) > 0;
						bool phoneExists = reader.GetInt32(2) > 0;
						reader.Close();

						if (usernameExists || emailExists || phoneExists)
						{
							var errors = new List<string>();
							if (usernameExists) errors.Add("Tên đăng nhập đã tồn tại");
							if (emailExists) errors.Add("Email đã tồn tại");
							if (phoneExists) errors.Add("Số điện thoại đã tồn tại");

							return BadRequest(new { message = "Đã có lỗi xảy ra", errors });
						}
					}
					else
					{
						reader.Close();
					}
				}

				// Thêm người dùng mới
				string insertQuery = @"
            INSERT INTO Users (Username, Email, FullName, Phone, Password, Address, RoleID)
            VALUES (@Username, @Email, @FullName, @Phone, @Password, @Address, @RoleID)";
				using (SqlCommand myCommand = new SqlCommand(insertQuery, myCon))
				{
					myCommand.Parameters.AddWithValue("@Username", customer.Username);
					myCommand.Parameters.AddWithValue("@Email", customer.Email);
					myCommand.Parameters.AddWithValue("@FullName", customer.FullName);
					myCommand.Parameters.AddWithValue("@Phone", customer.Phone);
					myCommand.Parameters.AddWithValue("@Password", customer.Password);
					myCommand.Parameters.AddWithValue("@Address", customer.Address);
					myCommand.Parameters.AddWithValue("@RoleID", customer.RoleID);
					myCommand.ExecuteNonQuery();
				}
			}
			return new JsonResult("Added Successfully");
		}
		// cập nhật
		[HttpPut("{id}")]
		public IActionResult UpdateCustomer(int id, [FromBody] Customer customer)
		{
			// Kiểm tra tính hợp lệ của dữ liệu người dùng
			if (customer == null)
			{
				return BadRequest(new { message = "Dữ liệu không hợp lệ." });
			}

			// Kiểm tra các trường bắt buộc
			if (string.IsNullOrEmpty(customer.Username) || string.IsNullOrEmpty(customer.Email) || string.IsNullOrEmpty(customer.Phone))
			{
				return BadRequest(new { message = "Username, Email và Phone là bắt buộc." });
			}

			// Tiến hành cập nhật
			string query = @"UPDATE Users 
                     SET Username = @Username, 
                         Email = @Email, 
                         FullName = @FullName, 
                         Phone = @Phone, 
                         Address = @Address, 
                         RoleID = @RoleID 
                     WHERE UserID = @UserID";
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			try
			{
				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();

					using (SqlCommand myCommand = new SqlCommand(query, myCon))
					{
						myCommand.Parameters.AddWithValue("@UserID", id);
						myCommand.Parameters.AddWithValue("@Username", customer.Username);
						myCommand.Parameters.AddWithValue("@Email", customer.Email);
						myCommand.Parameters.AddWithValue("@FullName", customer.FullName);
						myCommand.Parameters.AddWithValue("@Phone", customer.Phone);
						myCommand.Parameters.AddWithValue("@Address", customer.Address);
						myCommand.Parameters.AddWithValue("@RoleID", customer.RoleID);

						var rowsAffected = myCommand.ExecuteNonQuery();

						if (rowsAffected == 0)
						{
							return NotFound(new { message = "Không tìm thấy người dùng với ID đã cho." });
						}
					}
				}

				return Ok(new { message = "Cập nhật thông tin người dùng thành công." });
			}
			catch (SqlException sqlEx)
			{
				return StatusCode(500, new { message = "Lỗi cơ sở dữ liệu.", details = sqlEx.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật thông tin.", details = ex.Message });
			}
		}

		// Người dùng cập nhật thông tin cá nhân
		[HttpPut("user-profile/{id}")]
		public IActionResult UpdateUserProfile(int id, [FromBody] UpdateCustomerDto customer)
		{
			if (customer == null)
			{
				return BadRequest(new { message = "Dữ liệu không hợp lệ." });
			}

			if (string.IsNullOrEmpty(customer.Email) || string.IsNullOrEmpty(customer.Phone))
			{
				return BadRequest(new { message = "Email và Phone là bắt buộc." });
			}

			string query = @"
        UPDATE Users 
        SET 
            FullName = @FullName, 
            Phone = @Phone, 
            Address = @Address, 
            Email = @Email
        WHERE UserID = @UserID";

			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			try
			{
				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();
					using (SqlCommand myCommand = new SqlCommand(query, myCon))
					{
						myCommand.Parameters.AddWithValue("@UserID", id);
						myCommand.Parameters.AddWithValue("@FullName", customer.FullName ?? (object)DBNull.Value);
						myCommand.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
						myCommand.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
						myCommand.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);

						int rows = myCommand.ExecuteNonQuery();
						if (rows == 0)
							return NotFound(new { message = "Không tìm thấy người dùng." });
					}
				}
				return Ok(new { message = "Cập nhật thông tin cá nhân thành công." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Lỗi cập nhật thông tin.", details = ex.Message });
			}
		}



		// Xóa người dùng
		[HttpDelete("{id}")]
		public IActionResult DeleteCustomer(int id)
		{
			string query = "DELETE FROM Users WHERE UserID = @UserID";
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					myCommand.Parameters.AddWithValue("@UserID", id);
					myCommand.ExecuteNonQuery();
					myCon.Close();
				}
			}
			return new JsonResult("Deleted Successfully");
		}

		// Đổi mật khẩu
		[HttpPost("change-password")]
		public IActionResult ChangePassword([FromBody] ChangePasswordDto model)
		{
			if (model == null || string.IsNullOrWhiteSpace(model.OldPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
			{
				return BadRequest(new { message = "Dữ liệu không hợp lệ." });
			}

			string query = "SELECT Password FROM Users WHERE UserID = @UserId";
			string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE UserID = @UserId";
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			try
			{
				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();

					// Kiểm tra mật khẩu hiện tại có đúng không
					string currentPasswordFromDb = string.Empty;

					using (SqlCommand getPasswordCommand = new SqlCommand(query, myCon))
					{
						getPasswordCommand.Parameters.AddWithValue("@UserId", model.UserId);
						var result = getPasswordCommand.ExecuteScalar();
						if (result != null)
						{
							currentPasswordFromDb = result.ToString();
						}
						else
						{
							return NotFound(new { message = "Không tìm thấy người dùng." });
						}
					}

					if (currentPasswordFromDb != model.OldPassword)
					{
						return BadRequest(new { message = "Mật khẩu hiện tại không đúng." });
					}

					// Cập nhật mật khẩu mới
					using (SqlCommand updateCommand = new SqlCommand(updateQuery, myCon))
					{
						updateCommand.Parameters.AddWithValue("@NewPassword", model.NewPassword);
						updateCommand.Parameters.AddWithValue("@UserId", model.UserId);
						updateCommand.ExecuteNonQuery();
					}
				}

				return Ok(new { message = "Đổi mật khẩu thành công." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Đã xảy ra lỗi khi đổi mật khẩu.", details = ex.Message });
			}
		}

	}
}
