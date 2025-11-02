using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockCare.Data;
using StockCare.Models;

namespace StockCare.Controllers
{
    public class MovementsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MovementsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Movements
        public async Task<IActionResult> Index()
        {
            var movements = await _db.StockMovements.Include(m => m.Product).OrderByDescending(m => m.Date).ToListAsync();
            return View(movements);
        }

        // GET: Movements/Create
        public async Task<IActionResult> Create()
        {
            var products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name");
            return View();
        }

        // POST: Movements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Type,Quantity,Date,Notes")] StockMovement movement)
        {
            var product = await _db.Products.FindAsync(movement.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Producto no encontrado");
            }

            if (movement.Type == MovementType.Sale && product != null && movement.Quantity > product.CurrentStock)
            {
                ModelState.AddModelError(string.Empty, "Stock insuficiente para realizar la venta.");
            }

            if (ModelState.IsValid)
            {
                // Update product stock
                if (movement.Type == MovementType.Purchase)
                    product!.CurrentStock += movement.Quantity;
                else
                    product!.CurrentStock -= movement.Quantity;

                _db.StockMovements.Add(movement);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name", movement.ProductId);
            return View(movement);
        }

        // GET: Movements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movement = await _db.StockMovements.Include(m => m.Product).FirstOrDefaultAsync(m => m.Id == id);
            if (movement == null) return NotFound();

            return View(movement);
        }
    }
}
