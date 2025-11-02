using System.ComponentModel.DataAnnotations;

namespace StockCare.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [DataType(DataType.Currency)]
        public decimal PurchasePrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal SalePrice { get; set; }

        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        [Display(Name = "Minimum Stock")]
        public int MinimumStock { get; set; }

        public ICollection<StockMovement>? Movements { get; set; }
    }
}
