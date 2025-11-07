using Microsoft.EntityFrameworkCore;
using BTL_LTW.Models;

namespace BTL_LTW.Data
{
    public class RestaurantDb : DbContext
    {
        public RestaurantDb(DbContextOptions<RestaurantDb> options) : base(options) { }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<TableInfo> Tables { get; set; }
        public DbSet<TableInfo> TableInfos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}