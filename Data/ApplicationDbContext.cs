// Importa los espacios de nombres necesarios
// - Microsoft.EntityFrameworkCore: provee las clases base para trabajar con EF Core
// - StockCare.Models: contiene las clases de modelo (Product, StockMovement)
using Microsoft.EntityFrameworkCore;
using StockCare.Models;

namespace StockCare.Data
{
    // Clase principal del contexto de base de datos
    // Hereda de DbContext, lo que permite interactuar con la base de datos
    public class ApplicationDbContext : DbContext
    {
        // Constructor: recibe las opciones de configuración del contexto
        // Estas opciones (como la cadena de conexión) son inyectadas desde el startup del proyecto
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet representa una tabla en la base de datos
        // Cada propiedad corresponde a una colección de entidades del tipo indicado

        // Tabla de productos
        public DbSet<Product> Products { get; set; } = null!;

        // Tabla de movimientos de stock
        public DbSet<StockMovement> StockMovements { get; set; } = null!;

        // Método que se ejecuta cuando se está creando el modelo de base de datos
        // Aquí se definen las relaciones y comportamientos entre entidades
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Llama a la implementación base
            base.OnModelCreating(modelBuilder);

            // Configura la relación uno a muchos entre Product y StockMovement:
            // Un producto puede tener muchos movimientos
            // Cada movimiento pertenece a un producto
            // Si se elimina un producto, se eliminan también sus movimientos asociados (DeleteBehavior.Cascade)
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Movements)
                .WithOne(m => m.Product)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
