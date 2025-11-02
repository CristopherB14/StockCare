using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockCare.Models
{
    public enum MovementType
    {
        Purchase,
        Sale
    }

    public class StockMovement
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public MovementType Type { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
