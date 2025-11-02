using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockCare.Data;

namespace StockCare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products.ToListAsync();

            var lowStock = products.Where(p => p.CurrentStock < p.MinimumStock).ToList();

            // Report: most sold products
            var topSold = await _db.StockMovements
                .Where(m => m.Type == Models.MovementType.Sale)
                .GroupBy(m => m.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToListAsync();

            var topSoldWithProduct = new List<(string Name, int Quantity)>();
            foreach (var item in topSold)
            {
                var prod = await _db.Products.FindAsync(item.ProductId);
                if (prod != null)
                    topSoldWithProduct.Add((prod.Name, item.Quantity));
            }

            // Profitability: sold quantity * (sale - purchase)
            var profitability = await _db.StockMovements
                .Where(m => m.Type == Models.MovementType.Sale)
                .GroupBy(m => m.ProductId)
                .Select(g => new { ProductId = g.Key, SoldQty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            var profitList = new List<(string Name, int SoldQty, decimal ProfitPerUnit, decimal TotalProfit)>();
            foreach (var item in profitability)
            {
                var prod = await _db.Products.FindAsync(item.ProductId);
                if (prod != null)
                {
                    var profitPerUnit = prod.SalePrice - prod.PurchasePrice;
                    var total = profitPerUnit * item.SoldQty;
                    profitList.Add((prod.Name, item.SoldQty, profitPerUnit, total));
                }
            }

            var vm = new DashboardViewModel
            {
                TotalProducts = products.Count,
                LowStockCount = lowStock.Count,
                LowStockProducts = lowStock,
                TopSold = topSoldWithProduct,
                Profitability = profitList
            };

            return View(vm);
        }
    }

    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public List<StockCare.Models.Product> LowStockProducts { get; set; } = new();
        public List<(string Name, int Quantity)> TopSold { get; set; } = new();
        public List<(string Name, int SoldQty, decimal ProfitPerUnit, decimal TotalProfit)> Profitability { get; set; } = new();
    }
}
