namespace CakeShop_BE.Model
{
	public class Product
	{
		public int ProductID { get; set; }
		public string ProductName { get; set; }
		public int CategoryID { get; set; }
	//	public Category Category { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public string Description { get; set; }
		public string ImageURL { get; set; }

	}
}
