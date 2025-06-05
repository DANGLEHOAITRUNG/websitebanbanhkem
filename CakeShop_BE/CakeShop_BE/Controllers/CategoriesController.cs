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
	public class CategoriesController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public CategoriesController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		// 1. Lấy danh sách tất cả danh mục
		[HttpGet]
		public IActionResult GetAllCategories()
		{
			string query = "SELECT CategoryID, CategoryName FROM Categories";
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
				}
				myCon.Close();
			}
			return new JsonResult(table);
		}

		// 2. Lấy danh mục theo ID
		[HttpGet("{id}")]
		public IActionResult GetCategoryById(int id)
		{
			string query = "SELECT CategoryID, CategoryName FROM Categories WHERE CategoryID = @CategoryId";
			DataTable table = new DataTable();
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					myCommand.Parameters.AddWithValue("@CategoryId", id);
					SqlDataReader myReader = myCommand.ExecuteReader();
					table.Load(myReader);
					myReader.Close();
				}
				myCon.Close();
			}

			if (table.Rows.Count == 0)
			{
				return NotFound(new { message = "Không tìm thấy danh mục!" });
			}

			return new JsonResult(table.Rows[0]);
		}
		// 3. Thêm danh mục mới
		[HttpPost]
		public IActionResult AddCategory([FromBody] Category category)
		{
			string query = "INSERT INTO Categories (CategoryName) VALUES (@Categoryname)";
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					myCommand.Parameters.AddWithValue("@Categoryname", category.CategoryName);

					myCommand.ExecuteNonQuery();
				}
				myCon.Close();
			}
			return Ok(new { message = "Thêm danh mục thành công!" });
		}

		// 4. Cập nhật danh mục
		[HttpPut("{id}")]
		public IActionResult UpdateCategory(int id, [FromBody] Category category)
		{
			string query = "UPDATE Categories SET CategoryName = @Categoryname WHERE CategoryID = @CategoryId";
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					myCommand.Parameters.AddWithValue("@CategoryId", id);
					myCommand.Parameters.AddWithValue("@Categoryname", category.CategoryName);
					int rowsAffected = myCommand.ExecuteNonQuery();
					if (rowsAffected == 0)
					{
						return NotFound(new { message = "Không tìm thấy danh mục để cập nhật!" });
					}
				}
				myCon.Close();
			}
			return Ok(new { message = "Cập nhật danh mục thành công!" });
		}

		// 5. Xóa danh mục
		[HttpDelete("{id}")]
		public IActionResult DeleteCategory(int id)
		{
			string query = "DELETE FROM Categories WHERE CategoryID = @CategoryId";
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					myCommand.Parameters.AddWithValue("@CategoryId", id);

					int rowsAffected = myCommand.ExecuteNonQuery();
					if (rowsAffected == 0)
					{
						return NotFound(new { message = "Không tìm thấy danh mục để xóa!" });
					}
				}
				myCon.Close();
			}
			return Ok(new { message = "Xóa danh mục thành công!" });
		}
	}
}