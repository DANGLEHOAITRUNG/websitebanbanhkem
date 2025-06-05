using CakeShop_BE.Model;

namespace CakeShop_BE.DTO
{
	public class Products
	{
		public int ProductID { get; set; }
		public string? ProductName { get; set; }
		public int CategoryID { get; set; }
		public string CategoryName { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public string Description { get; set; }
		public string ImageURL { get; set; }
	}
}
