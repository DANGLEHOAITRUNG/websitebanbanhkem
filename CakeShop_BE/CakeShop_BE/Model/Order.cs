using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CakeShop_BE.Model
{
	public class Order
	{
		[Key]
		public int OrderID { get; set; }
		public int UserID { get; set; }

		public string FullName { get; set; }
		public string Phone { get; set; }
		public string Address { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string Status { get; set; }

		public string PaymentMethod { get; set; }
		public string? PaymentProofImage { get; set; }
	}
}
