namespace CakeShop_BE.Model
{
	public class Cart
	{
		public int CartID { get; set; }    // ID của giỏ hàng (nếu cần, có thể không bắt buộc)
		public int UserID { get; set; }    // ID của người dùng
		public int ProductID { get; set; } // ID của sản phẩm
		public int Quantity { get; set; }  // Số lượng sản phẩm
	}
}
