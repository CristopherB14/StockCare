/* Explicación general del flujo:
Acción	        Método	    Descripción
Index()	            GET	    Lista todos los productos.
Details(id)	        GET	    Muestra detalles y movimientos del producto.
Create()	        GET	    Muestra el formulario de creación.
Create(product)	    POST	Crea un nuevo producto (guarda en DB).
Edit(id)	        GET	    Muestra el formulario para editar un producto.
Edit(id, product)	POST	Guarda los cambios realizados.
Delete(id)	        GET	    Muestra confirmación de eliminación.
DeleteConfirmed(id)	POST	Elimina el producto de la base.*/

// Espacios de nombres necesarios para que el controlador funcione correctamente.
using Microsoft.AspNetCore.Mvc;       // Proporciona clases base para controladores y vistas (MVC).
using Microsoft.EntityFrameworkCore;  // Permite interactuar con la base de datos usando Entity Framework.
using StockCare.Data;                 // Contiene el ApplicationDbContext, nuestro contexto de base de datos.
using StockCare.Models;               // Contiene las clases del modelo (Product, etc.).

namespace StockCare.Controllers
{
    // Este controlador maneja todas las operaciones CRUD (Crear, Leer, Actualizar y Eliminar)
    // relacionadas con los productos dentro del sistema.
    public class ProductsController : Controller
    {
        // Campo privado donde se guarda el contexto de base de datos.
        private readonly ApplicationDbContext _db;

        // Constructor del controlador, recibe el contexto mediante inyección de dependencias.
        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ---------------------------------------------------------------
        // GET: Products
        // Muestra la lista de todos los productos almacenados en la base de datos.
        // ---------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            // Cargamos todos los productos de forma asincrónica.
            var products = await _db.Products.ToListAsync();

            // Enviamos la lista de productos a la vista para mostrarlos en una tabla.
            return View(products);
        }

        // ---------------------------------------------------------------
        // GET: Products/Details/5
        // Muestra los detalles de un producto específico, incluyendo sus movimientos de stock.
        // ---------------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            // Si no se pasó un ID, devolvemos error 404 (no encontrado).
            if (id == null) return NotFound();

            // Buscamos el producto y cargamos también sus movimientos asociados.
            var product = await _db.Products
                .Include(p => p.Movements)             // Incluye la lista de movimientos (ventas, compras).
                .FirstOrDefaultAsync(p => p.Id == id); // Busca el producto con ese ID.

            // Si no se encontró el producto, también devolvemos error 404.
            if (product == null) return NotFound();

            // Enviamos el producto a la vista para mostrar sus datos.
            return View(product);
        }

        // ---------------------------------------------------------------
        // GET: Products/Create
        // Muestra el formulario para crear un nuevo producto.
        // ---------------------------------------------------------------
        public IActionResult Create()
        {
            // Simplemente mostramos la vista vacía del formulario.
            return View();
        }

        // ---------------------------------------------------------------
        // POST: Products/Create
        // Recibe los datos del formulario y crea un nuevo producto en la base de datos.
        // ---------------------------------------------------------------
        [HttpPost] // Indica que este método maneja peticiones POST (formulario enviado).
        [ValidateAntiForgeryToken] // Protege contra ataques CSRF (seguridad).
        public async Task<IActionResult> Create([Bind("Name,Description,Category,PurchasePrice,SalePrice,CurrentStock,MinimumStock")] Product product)
        {
            // Verificamos si los datos ingresados cumplen las validaciones del modelo.
            if (ModelState.IsValid)
            {
                // Agregamos el nuevo producto a la base de datos.
                _db.Add(product);

                // Guardamos los cambios de forma asincrónica.
                await _db.SaveChangesAsync();

                // Redirigimos a la lista de productos.
                return RedirectToAction(nameof(Index));
            }

            // Si hubo errores de validación, mostramos nuevamente el formulario con los mensajes de error.
            return View(product);
        }

        // ---------------------------------------------------------------
        // GET: Products/Edit/5
        // Muestra el formulario para editar un producto existente.
        // ---------------------------------------------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            // Si no se pasó un ID, devolvemos error 404.
            if (id == null) return NotFound();

            // Buscamos el producto a editar.
            var product = await _db.Products.FindAsync(id);

            // Si no existe, devolvemos error 404.
            if (product == null) return NotFound();

            // Enviamos el producto a la vista para editarlo.
            return View(product);
        }

        // ---------------------------------------------------------------
        // POST: Products/Edit/5
        // Recibe los datos modificados y actualiza el producto en la base de datos.
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Category,PurchasePrice,SalePrice,CurrentStock,MinimumStock")] Product product)
        {
            // Si el ID de la URL no coincide con el del producto enviado, devolvemos error.
            if (id != product.Id) return NotFound();

            // Si el modelo es válido (sin errores de validación)...
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizamos el producto en la base de datos.
                    _db.Update(product);

                    // Guardamos los cambios.
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Si otro proceso modificó el mismo producto al mismo tiempo,
                    // verificamos si el producto todavía existe.
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw; // Si existe pero hay conflicto, relanzamos el error.
                }

                // Redirigimos a la lista de productos.
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, volvemos a mostrar la vista de edición con los datos actuales.
            return View(product);
        }

        // ---------------------------------------------------------------
        // GET: Products/Delete/5
        // Muestra una confirmación antes de eliminar el producto.
        // ---------------------------------------------------------------
        public async Task<IActionResult> Delete(int? id)
        {
            // Si no se pasó un ID, devolvemos error.
            if (id == null) return NotFound();

            // Buscamos el producto a eliminar.
            var product = await _db.Products.FirstOrDefaultAsync(m => m.Id == id);

            // Si no se encuentra, devolvemos error.
            if (product == null) return NotFound();

            // Enviamos los datos del producto a la vista de confirmación.
            return View(product);
        }

        // ---------------------------------------------------------------
        // POST: Products/Delete/5
        // Se ejecuta cuando el usuario confirma la eliminación del producto.
        // ---------------------------------------------------------------
        [HttpPost, ActionName("Delete")] // "ActionName" permite que la vista llame a este método con el nombre "Delete".
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Buscamos el producto nuevamente (por seguridad).
            var product = await _db.Products.FindAsync(id);

            // Si existe, lo eliminamos de la base de datos.
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }

            // Redirigimos a la lista de productos actualizada.
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------
        // Método auxiliar (privado) para verificar si un producto existe.
        // ---------------------------------------------------------------
        private bool ProductExists(int id)
        {
            // Devuelve true si hay un producto con ese ID, o false si no.
            return _db.Products.Any(e => e.Id == id);
        }
    }
}
