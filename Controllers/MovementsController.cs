/*🧠 Explicación general del flujo:

--Index(): muestra todos los movimientos (compras y ventas) en una tabla.
--Create() [GET]: carga el formulario vacío para registrar un nuevo movimiento.
--Create() [POST]: recibe los datos del formulario, valida la información, actualiza el stock del producto y guarda los cambios.
--Details(): muestra información detallada sobre un movimiento en particular.*/

// Espacios de nombres necesarios para que el controlador funcione correctamente.
using Microsoft.AspNetCore.Mvc;             // Proporciona clases y métodos base para crear controladores MVC.
using Microsoft.AspNetCore.Mvc.Rendering;   // Permite crear listas desplegables (SelectList) en las vistas.
using Microsoft.EntityFrameworkCore;        // Permite usar Entity Framework Core para acceder a la base de datos.
using StockCare.Data;                       // Espacio de nombres donde está definido el ApplicationDbContext.
using StockCare.Models;                     // Contiene las clases del modelo, como Product y StockMovement.

namespace StockCare.Controllers
{
    // Este controlador maneja las operaciones relacionadas con los movimientos de stock:
    // por ejemplo, registrar compras o ventas de productos.
    public class MovementsController : Controller
    {
        // Campo privado que guarda una referencia al contexto de base de datos.
        private readonly ApplicationDbContext _db;

        // Constructor que recibe el contexto mediante inyección de dependencias.
        public MovementsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Movements
        // Muestra una lista de todos los movimientos de stock (ventas y compras).
        public async Task<IActionResult> Index()
        {
            // Cargamos todos los movimientos desde la base de datos.
            // Incluimos el producto relacionado con cada movimiento (Include),
            // y ordenamos de más reciente a más antiguo.
            var movements = await _db.StockMovements
                .Include(m => m.Product)                // Trae también los datos del producto relacionado.
                .OrderByDescending(m => m.Date)         // Ordena los resultados por fecha descendente.
                .ToListAsync();                         // Ejecuta la consulta de forma asincrónica.

            // Enviamos la lista de movimientos a la vista para mostrarla al usuario.
            return View(movements);
        }

        // GET: Movements/Create
        // Muestra el formulario para crear un nuevo movimiento de stock.
        public async Task<IActionResult> Create()
        {
            // Obtenemos todos los productos, ordenados por nombre.
            var products = await _db.Products.OrderBy(p => p.Name).ToListAsync();

            // Creamos una lista desplegable para el selector de productos en la vista.
            // El primer parámetro es la lista de objetos,
            // el segundo es el campo "Id" (valor del option),
            // y el tercero es "Name" (texto visible al usuario).
            ViewBag.Products = new SelectList(products, "Id", "Name");

            // Devolvemos la vista del formulario vacío.
            return View();
        }

        // POST: Movements/Create
        // Este método se ejecuta cuando el usuario envía el formulario de creación.
        [HttpPost] // Indica que este método responde a peticiones POST.
        [ValidateAntiForgeryToken] // Protege contra ataques de falsificación de solicitudes (CSRF).
        public async Task<IActionResult> Create([Bind("ProductId,Type,Quantity,Date,Notes")] StockMovement movement)
        {
            // Buscamos el producto asociado al movimiento.
            var product = await _db.Products.FindAsync(movement.ProductId);

            // Si el producto no existe, agregamos un error al modelo.
            if (product == null)
            {
                ModelState.AddModelError("ProductId", "Producto no encontrado");
            }

            // Validación adicional:
            // Si el movimiento es una venta y la cantidad vendida supera el stock disponible, mostramos error.
            if (movement.Type == MovementType.Sale && product != null && movement.Quantity > product.CurrentStock)
            {
                ModelState.AddModelError(string.Empty, "Stock insuficiente para realizar la venta.");
            }

            // Si no hubo errores de validación...
            if (ModelState.IsValid)
            {
                // Actualizamos el stock del producto según el tipo de movimiento.
                if (movement.Type == MovementType.Purchase)
                    // Si es una compra, sumamos al stock actual.
                    product!.CurrentStock += movement.Quantity;
                else
                    // Si es una venta, restamos del stock actual.
                    product!.CurrentStock -= movement.Quantity;

                // Agregamos el nuevo movimiento a la base de datos.
                _db.StockMovements.Add(movement);

                // Guardamos todos los cambios realizados (movimiento + stock actualizado).
                await _db.SaveChangesAsync();

                // Redirigimos al usuario de nuevo a la lista de movimientos.
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo no es válido (hubo errores),
            // volvemos a preparar la lista de productos para el formulario.
            var products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name", movement.ProductId);

            // Volvemos a mostrar la vista con los datos ingresados y los mensajes de error.
            return View(movement);
        }

        // GET: Movements/Details/5
        // Muestra los detalles de un movimiento específico.
        public async Task<IActionResult> Details(int? id)
        {
            // Si el id no fue proporcionado, devolvemos un error 404.
            if (id == null) return NotFound();

            // Buscamos el movimiento en la base de datos,
            // incluyendo los datos del producto relacionado.
            var movement = await _db.StockMovements
                .Include(m => m.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Si el movimiento no existe, devolvemos otro error 404.
            if (movement == null) return NotFound();

            // Si todo está bien, enviamos el movimiento a la vista para mostrarlo.
            return View(movement);
        }
    }
}
