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
	public class UsersController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public UsersController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpPost("dangnhap")]
		public JsonResult DN([FromBody] Users userModel)
		{
			try
			{
				string query = @"
			SELECT u.UserID, u.Username, u.Password, u.RoleID, r.RoleName
			FROM Users u
			JOIN Roles r ON u.RoleID = r.RoleID
			WHERE u.Username = @username AND u.Password = @password";

				string sqlDataSource = _configuration.GetConnectionString("CakeShop");
				int userID = 0;
				string username = string.Empty;
				string roleID = string.Empty;
				string roleName = string.Empty;

				using (SqlConnection myCon = new SqlConnection(sqlDataSource))
				{
					myCon.Open();
					using (SqlCommand myCommand = new SqlCommand(query, myCon))
					{
						myCommand.Parameters.AddWithValue("@username", userModel.Username);
						myCommand.Parameters.AddWithValue("@password", userModel.Password);

						using (SqlDataReader reader = myCommand.ExecuteReader())
						{
							if (reader.Read())
							{
								userID = Convert.ToInt32(reader["UserID"]);
								username = reader["Username"].ToString();
								roleID = reader["RoleID"].ToString();
								roleName = reader["RoleName"].ToString();
							}
						}
					}
				}

				if (!string.IsNullOrEmpty(username))
				{
					return new JsonResult(new
					{
						status = "success",
						message = "Login successful",
						data = new
						{
							UserID = userID,
							Username = username,
							RoleID = roleID,
							RoleName = roleName
						}
					});
				}
				else
				{
					return new JsonResult(new { status = "failure", message = "Invalid username or password" });
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