using EcommerceApp.Entities;
using EcommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
       public DbSet<EmailCode>EmailCodes { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<DiscountSetting> DiscountSettings { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; }=null!;
     public DbSet<ClothingItem>ClothingItems { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<ShippingCompany> ShippingCompanies { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<SearchHistory> SearchHistories { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<CompartibleColor>CompartibleColors { get; set; } = null!;

        public DbSet<OtpCode> OtpCodes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>() .HasOne(o => o.Payment) .WithOne(p => p.Order).HasForeignKey<Payment>(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>().HasOne(e => e.ShippingCompany).WithMany(e=>e.Orders).HasForeignKey(e => e.ShippingCompanyId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>().HasOne(e => e.User).WithMany(e=>e.Orders).HasForeignKey(e=>e.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SearchHistory>().HasOne(e => e.User).WithMany(a=>a.SearchHistories).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<OrderItem>().HasOne(e=>e.Order).WithMany(e=>e.OrderItems).HasForeignKey(e=>e.OrderId).OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<OrderItem>().HasOne(e => e.Product).WithMany(e => e.OrderItems).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Product>().HasOne(e=>e.Category).WithMany(e=>e.Products).HasForeignKey(e=>e.CategoryId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>().HasOne(e=>e.DiscountSetting).WithMany(e=>e.Products).HasForeignKey(e=>e.DiscountSettingId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>().HasOne(e => e.Product).WithMany(e=>e.CartItems).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>().HasOne(e => e.Cart).WithMany(e=>e.CartItems).HasForeignKey(e => e.CartId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>().HasOne(e => e.Product).WithMany(e=>e.Favorites).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>().HasOne(e=>e.User).WithMany(e=>e.Favorites).HasForeignKey(e=>e.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Cart>().HasOne(e=>e.User).WithOne(e=>e.Cart).HasForeignKey<Cart>(e=>e.UserId).OnDelete(DeleteBehavior.Restrict);
         //   modelBuilder.Entity<ClothingItem>().HasOne(e=>e.Product).WithMany(e=>e.ClothingItems).HasForeignKey(e=>e.ProductId).OnDelete(DeleteBehavior.Restrict);

        }

     


        

    }
}
