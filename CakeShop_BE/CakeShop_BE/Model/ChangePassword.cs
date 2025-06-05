namespace CakeShop_BE.Model
{
	public class ChangePassword
	{
		public int UserId { get; set; }
		public string CurrentPassword { get; set; } = string.Empty;
		public string NewPassword { get; set; } = string.Empty;
	}
}
