using Microsoft.EntityFrameworkCore;
using StockCare.Models;

namespace StockCare.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<StockMovement> StockMovements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Movements)
                .WithOne(m => m.Product)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
