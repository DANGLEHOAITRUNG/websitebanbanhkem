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

namespace CakeShop_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReUsersController : ControllerBase
    {
		private readonly IConfiguration _configuration;

		public ReUsersController(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		//DK
		[HttpPost("dangky")]
		public JsonResult Register([FromBody] ReUsers reUsersModel)
		{
			try
			{

				//  Kiểm tra nếu tên người dùng đã tồn tại
				string queryCheckUser = "SELECT COUNT(*) AS UserCount FROM Users WHERE Username = @username";
				int userCount = 0;
				string sqlDataSource = _configuration.GetConnectionString("CakeShop");

				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();
					using (SqlCommand myCommand = new SqlCommand(queryCheckUser, myCon))
					{
						myCommand.Parameters.AddWithValue("@username", reUsersModel.Username);
						userCount = (int)myCommand.ExecuteScalar();
					}
				}

				if (userCount > 0)
				{
					return new JsonResult(new { status = "failure", message = "Username already exists" });
				}

				//  Mã hóa mật khẩu trước khi lưu
				//string hashedPassword;
				//using (SHA256 sha256 = SHA256.Create())
				//{
				//    byte[] passwordBytes = Encoding.UTF8.GetBytes(reUsersModel.Password);
				//    byte[] hashBytes = sha256.ComputeHash(passwordBytes);
				//    hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
				//}

				//  Thêm người dùng vào cơ sở dữ liệu với vai trò mặc định là 'user'
				string queryInsertUsers = @"
                    INSERT INTO Users (Username, Email, Phone, Password, Address, RoleID) 
                    VALUES (@username, @email, @phone, @password, @address, 
                            (SELECT RoleID FROM Roles WHERE RoleName = 'Customer'  ))";

				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();
					using (SqlCommand myCommand = new SqlCommand(queryInsertUsers, myCon))
					{
						myCommand.Parameters.AddWithValue("@username", reUsersModel.Username);
						myCommand.Parameters.AddWithValue("@email", reUsersModel.Email);
						myCommand.Parameters.AddWithValue("@phone", reUsersModel.Phone);
						myCommand.Parameters.AddWithValue("@password", reUsersModel.Password);
						myCommand.Parameters.AddWithValue("@address", reUsersModel.Address);

						int result = myCommand.ExecuteNonQuery();
						if (result > 0)
						{
							return new JsonResult(new { status = "success", message = "Registration successful" });
						}
						else
						{
							return new JsonResult(new { status = "failure", message = "An error occurred while registering" });
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return new JsonResult(new { status = "error", message = $"An error occurred: {ex.Message}" });
			}
		}

	}
}