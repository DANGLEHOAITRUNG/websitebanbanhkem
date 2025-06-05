using System.ComponentModel.DataAnnotations;

namespace CakeShop_BE.Model
{
	public class Customer
	{
		[Key]
		public int UserID { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		public string Phone { get; set; }
		public string Password { get; set; }
		public string Address { get; set; }
		public int RoleID { get; set; }
	}
}
