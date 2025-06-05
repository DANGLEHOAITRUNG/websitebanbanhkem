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
using CakeShop_BE.DTO;
using Microsoft.CodeAnalysis;
using System.Data;

namespace CakeShop_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductsController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public ProductsController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpGet]
		public IActionResult GetAllProducts()
		{
			string query = @"
            SELECT p.ProductID, p.ProductName, p.Price, p.Quantity, p.Description, p.ImageURL,
                   c.CategoryID, c.CategoryName
            FROM Products p
            JOIN Categories c ON p.CategoryID = c.CategoryID";

			List<Products> products = new List<Products>();
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					SqlDataReader myReader = myCommand.ExecuteReader();
					while (myReader.Read())
					{
						var product = new Products
						{
							ProductID = Convert.ToInt32(myReader["ProductID"]),
							ProductName = myReader["ProductName"].ToString(),
							Price = Convert.ToDecimal(myReader["Price"]),
							Quantity = Convert.ToInt32(myReader["Quantity"]),
							Description = myReader["Description"].ToString(),
							ImageURL = myReader["ImageURL"].ToString(),
							CategoryID = Convert.ToInt32(myReader["CategoryID"]),
							CategoryName = myReader["CategoryName"].ToString()
						};
						products.Add(product);
					}
				}
				myCon.Close();
			}

			return Ok(products); 
		}


		[HttpGet("{id}")]
		public IActionResult GetProductById(int id)
		{
			string query = @"
            SELECT p.ProductID, p.ProductName, p.Price, p.Quantity, p.Description, p.ImageURL,
                   c.CategoryID, c.CategoryName
            FROM Products p
            JOIN Categories c ON p.CategoryID = c.CategoryID
            WHERE p.ProductID = @ProductID";

			Products product = null;
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					myCommand.Parameters.AddWithValue("@ProductID", id);
					SqlDataReader myReader = myCommand.ExecuteReader();
					if (myReader.Read())
					{
						product = new Products
						{
							ProductID = Convert.ToInt32(myReader["ProductID"]),
							ProductName = myReader["ProductName"].ToString(),
							Price = Convert.ToDecimal(myReader["Price"]),
							Quantity = Convert.ToInt32(myReader["Quantity"]),
							Description = myReader["Description"].ToString(),
							ImageURL = myReader["ImageURL"].ToString(),
							CategoryID = Convert.ToInt32(myReader["CategoryID"]),
							CategoryName = myReader["CategoryName"].ToString()
						};
					}
				}
				myCon.Close();
			}

			if (product == null)
			{
				return NotFound(new { message = "Không tìm thấy sản phẩm!" });
			}
			return Ok(product); // Trả về sản phẩm dưới dạng JSON
		}

		// Thêm sản phẩm
		[HttpPost]
		public IActionResult AddProduct([FromBody] Product product)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();

				// Kiểm tra tên sản phẩm đã tồn tại chưa
				string checkQuery = "SELECT COUNT(1) FROM Products WHERE ProductName = @ProductName";
				using (SqlCommand checkCommand = new SqlCommand(checkQuery, myCon))
				{
					checkCommand.Parameters.AddWithValue("@ProductName", product.ProductName);
					int count = (int)checkCommand.ExecuteScalar();
					if (count > 0)
					{
						return BadRequest(new { message = "Tên sản phẩm đã tồn tại" });
					}
				}

				// Thêm sản phẩm
				string insertQuery = @"
        INSERT INTO Products (ProductName, Price, Quantity, CategoryID, Description, ImageURL)
        VALUES (@ProductName, @Price, @Quantity, @CategoryID, @Description, @ImageURL)";

				using (SqlCommand insertCommand = new SqlCommand(insertQuery, myCon))
				{
					insertCommand.Parameters.AddWithValue("@ProductName", product.ProductName);
					insertCommand.Parameters.AddWithValue("@Price", product.Price);
					insertCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
					insertCommand.Parameters.AddWithValue("@CategoryID", product.CategoryID);
					insertCommand.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
					insertCommand.Parameters.AddWithValue("@ImageURL", product.ImageURL ?? (object)DBNull.Value);
					insertCommand.ExecuteNonQuery();
				}
			}
			return Ok(new { message = "Thêm sản phẩm thành công!" });
		}

		// Cập nhật sản phẩm
		[HttpPut("{id}")]
		public IActionResult UpdateProduct(int id, [FromBody] Product product)
		{
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();

				// Kiểm tra tên sản phẩm đã tồn tại chưa (trừ sản phẩm hiện tại)
				string checkQuery = "SELECT COUNT(1) FROM Products WHERE ProductName = @ProductName AND ProductID != @ProductID";
				using (SqlCommand checkCommand = new SqlCommand(checkQuery, myCon))
				{
					checkCommand.Parameters.AddWithValue("@ProductName", product.ProductName);
					checkCommand.Parameters.AddWithValue("@ProductID", id);
					int count = (int)checkCommand.ExecuteScalar();
					if (count > 0)
					{
						return BadRequest(new { message = "Tên sản phẩm đã tồn tại" });
					}
				}

				// Cập nhật sản phẩm
				string updateQuery = @"
            UPDATE Products
            SET ProductName = @ProductName, Price = @Price, Quantity = @Quantity,
                CategoryID = @CategoryID, Description = @Description, ImageURL = @ImageURL
            WHERE ProductID = @ProductID";

				using (SqlCommand updateCommand = new SqlCommand(updateQuery, myCon))
				{
					updateCommand.Parameters.AddWithValue("@ProductID", id);
					updateCommand.Parameters.AddWithValue("@ProductName", product.ProductName);
					updateCommand.Parameters.AddWithValue("@Price", product.Price);
					updateCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
					updateCommand.Parameters.AddWithValue("@CategoryID", product.CategoryID);
					updateCommand.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
					updateCommand.Parameters.AddWithValue("@ImageURL", product.ImageURL ?? (object)DBNull.Value);
					updateCommand.ExecuteNonQuery();
				}
			}
			return Ok(new { message = "Cập nhật sản phẩm thành công!" });
		}

		//  Xóa sản phẩm
		[HttpDelete("{id}")]
		public IActionResult DeleteProduct(int id)
		{
			string connectionString = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				SqlTransaction transaction = connection.BeginTransaction();

				try
				{
					// 1. Xóa các OrderDetails liên quan đến ProductID
					string deleteOrderDetailsQuery = "DELETE FROM OrderDetails WHERE ProductID = @ProductID";
					using (SqlCommand cmd = new SqlCommand(deleteOrderDetailsQuery, connection, transaction))
					{
						cmd.Parameters.AddWithValue("@ProductID", id);
						cmd.ExecuteNonQuery();
					}

					// 2. Xóa sản phẩm
					string deleteProductQuery = "DELETE FROM Products WHERE ProductID = @ProductID";
					using (SqlCommand cmd = new SqlCommand(deleteProductQuery, connection, transaction))
					{
						cmd.Parameters.AddWithValue("@ProductID", id);
						int affectedRows = cmd.ExecuteNonQuery();

						if (affectedRows == 0)
						{
							transaction.Rollback();
							return NotFound(new { message = "Không tìm thấy sản phẩm để xóa!" });
						}
					}

					transaction.Commit();
					return Ok(new { message = "Xóa sản phẩm và các chi tiết đơn hàng liên quan thành công!" });
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa sản phẩm.", error = ex.Message });
				}
				finally
				{
					connection.Close();
				}
			}
		}

		// Tìm kiếm sản phẩm theo tên
		[HttpGet("search")]
		public IActionResult SearchProductsByName(string name)
		{
			string query = @"
        SELECT p.ProductID, p.ProductName, p.Price, p.Quantity, p.Description, p.ImageURL,
               c.CategoryID, c.CategoryName
        FROM Products p
        JOIN Categories c ON p.CategoryID = c.CategoryID
        WHERE p.ProductName LIKE @ProductName";

			DataTable table = new DataTable();
			string sqlDataSource = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection myCon = new SqlConnection(sqlDataSource))
			{
				myCon.Open();
				using (SqlCommand myCommand = new SqlCommand(query, myCon))
				{
					// Tìm kiếm với từ khóa
					myCommand.Parameters.AddWithValue("@ProductName", "%" + name + "%");

					SqlDataReader myReader = myCommand.ExecuteReader();
					table.Load(myReader);
				}
				myCon.Close();
			}

			if (table.Rows.Count == 0)
			{
				return NotFound(new { message = "Không tìm thấy sản phẩm nào!" });
			}

			return new JsonResult(table);
		}

		// Lọc sản phẩm danh mục theo tên

		[HttpGet("category/{categoryName}")]
		public IActionResult GetProductsByCategory(string categoryName)
		{
			List<Products> products = new List<Products>();
			string connectionString = _configuration.GetConnectionString("CakeShop");

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
				SELECT p.ProductID, p.ProductName, p.CategoryID, c.CategoryName, 
					   p.Price, p.Quantity, p.Description, p.ImageURL
				FROM Products p
				INNER JOIN Categories c ON p.CategoryID = c.CategoryID
				WHERE c.CategoryName = @CategoryName";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@CategoryName", categoryName);
					conn.Open();

					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Products product = new Products
							{
								ProductID = Convert.ToInt32(reader["ProductID"]),
								ProductName = reader["ProductName"].ToString(),
								CategoryID = Convert.ToInt32(reader["CategoryID"]),
								CategoryName = reader["CategoryName"].ToString(),
								Price = Convert.ToDecimal(reader["Price"]),
								Quantity = Convert.ToInt32(reader["Quantity"]),
								Description = reader["Description"].ToString(),
								ImageURL = reader["ImageURL"].ToString()
							};
							products.Add(product);
						}
					}
				}
			}

			if (products.Count == 0)
				return NotFound("Không tìm thấy sản phẩm trong danh mục này.");

			return Ok(products);
		}
	}
}