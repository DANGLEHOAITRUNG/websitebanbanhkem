using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CakeShop_BE.Model
{
	public class OrderDetail
	{
		public int OrderDetailID { get; set; } // Nếu cần ID riêng
		public int OrderID { get; set; }
		public int ProductID { get; set; }
		public string ProductName { get; set; } // Thêm để hiển thị
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}
}
