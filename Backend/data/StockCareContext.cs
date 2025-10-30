using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class StockCareContext : DbContext
    {
        public StockCareContext(DbContextOptions<StockCareContext> options) 
            : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        // Agregá más tablas según tu SQL:
        // public DbSet<Producto> Productos { get; set; }
        // public DbSet<Venta> Ventas { get; set; }
    }
}
