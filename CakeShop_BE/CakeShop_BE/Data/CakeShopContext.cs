using CakeShop_BE.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CakeShop_BE.Data
{
	public class CakeShopContext : DbContext
	{
		public CakeShopContext(DbContextOptions<CakeShopContext> options) : base(options) { }

		public DbSet<Users> Users { get; set; }
		public DbSet<Customer> Customers { get; set; }
		public DbSet<ReUsers> ReUsers { get; set; }

		public DbSet<Cart> Carts { get; set; }

		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderDetail> OrderDetails { get; set; }

		public DbSet<CakeShop_BE.Model.Product> Product { get; set; } = default!;
	    public DbSet<CakeShop_BE.Model.Category> Category { get; set; } = default!;
	}
}
