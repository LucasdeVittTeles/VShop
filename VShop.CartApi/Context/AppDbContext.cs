using Microsoft.EntityFrameworkCore;
using VShop.CartApi.Models;

namespace VShop.CartApi.Context
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Product>? Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartHeader> CartHeaders { get; set; }

        //Fluent API

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //Product
            modelBuilder.Entity<Product>().HasKey(c => c.Id);

            //Product
            modelBuilder.Entity<Product>().Property(c => c.Id).ValueGeneratedNever();


        }

    }
}
