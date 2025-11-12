/*🧠 Explicación general del flujo:

--El usuario entra a la página principal (/Home/Index).
--El controlador se conecta a la base de datos mediante Entity Framework.
--Obtiene los datos necesarios: productos, ventas, stock, ganancias.
--Procesa esos datos (cálculos de top ventas, ganancias, stock bajo, etc.).
--Crea un modelo (DashboardViewModel) que contiene toda la información ya lista.
--Devuelve la vista “Index” junto con ese modelo, para que el HTML muestre las estadísticas en pantalla.*/

// Importamos los espacios de nombres necesarios
// Estos "using" permiten usar clases y métodos de otros módulos.

using Microsoft.AspNetCore.Mvc;      // Permite crear controladores y vistas MVC.
using Microsoft.EntityFrameworkCore; // Permite usar Entity Framework para acceder a bases de datos.
using StockCare.Data;                // Espacio de nombres donde está nuestro contexto de base de datos (ApplicationDbContext).

namespace StockCare.Controllers
{
    // Un controlador en ASP.NET Core MVC se encarga de manejar las solicitudes (requests)
    // que llegan desde el navegador (por ejemplo, al entrar a la página principal).
        public class HomeController : Controller
    {
        // Campo privado donde guardamos una referencia al contexto de la base de datos.
        private readonly ApplicationDbContext _db;

        // Constructor del controlador.
        // Se llama automáticamente cuando se crea el controlador.
        // Aquí recibimos por "inyección de dependencias" el contexto de base de datos.
        public HomeController(ApplicationDbContext db)
        {
            _db = db; // Guardamos el contexto en la variable privada para usarlo en otros métodos.
        }

        // Este método maneja la acción "Index", o sea, la página principal del Home.
        // Es asincrónico porque realiza operaciones que consultan la base de datos.
        public async Task<IActionResult> Index()
        {
            // 1️⃣ Obtenemos todos los productos de la base de datos.
            var products = await _db.Products.ToListAsync();

            // 2️⃣ Filtramos los productos con poco stock:
            // "CurrentStock" (stock actual) menor que "MinimumStock" (stock mínimo).
            var lowStock = products.Where(p => p.CurrentStock < p.MinimumStock).ToList();

            // 3️⃣ Reporte: Productos más vendidos
            // Consultamos los movimientos de stock (ventas), agrupamos por producto
            // y sumamos la cantidad total vendida.
            var topSold = await _db.StockMovements
                .Where(m => m.Type == Models.MovementType.Sale)       // Solo tomamos los movimientos que son ventas.
                .GroupBy(m => m.ProductId)                            // Agrupamos por producto.
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) }) // Calculamos la cantidad total vendida por producto.
                .OrderByDescending(x => x.Quantity)                   // Ordenamos del más vendido al menos vendido.
                .Take(10)                                             // Tomamos solo los 10 primeros.
                .ToListAsync();                                       // Ejecutamos la consulta y convertimos a lista.

            // 4️⃣ Creamos una lista con los nombres de los productos más vendidos.
            // (Porque en la consulta anterior solo tenemos el ID del producto).
            var topSoldWithProduct = new List<(string Name, int Quantity)>();
            foreach (var item in topSold)
            {
                // Buscamos el producto en la base de datos usando su ID.
                var prod = await _db.Products.FindAsync(item.ProductId);

                // Si el producto existe (por si acaso fue eliminado), lo agregamos a la lista.
                if (prod != null)
                    topSoldWithProduct.Add((prod.Name, item.Quantity));
            }

            // 5️⃣ Reporte de rentabilidad
            // Calculamos cuánto gana la empresa por cada producto vendido:
            // cantidad vendida * (precio de venta - precio de compra).
            var profitability = await _db.StockMovements
                .Where(m => m.Type == Models.MovementType.Sale)       // Solo movimientos de venta.
                .GroupBy(m => m.ProductId)                            // Agrupamos por producto.
                .Select(g => new { ProductId = g.Key, SoldQty = g.Sum(x => x.Quantity) }) // Sumamos las cantidades vendidas.
                .ToListAsync();

            // 6️⃣ Creamos una lista para guardar los resultados con nombre del producto,
            // cantidad vendida, ganancia por unidad y ganancia total.
            var profitList = new List<(string Name, int SoldQty, decimal ProfitPerUnit, decimal TotalProfit)>();
            foreach (var item in profitability)
            {
                var prod = await _db.Products.FindAsync(item.ProductId);
                if (prod != null)
                {
                    // Calculamos la ganancia por unidad.
                    var profitPerUnit = prod.SalePrice - prod.PurchasePrice;

                    // Ganancia total = ganancia por unidad * cantidad vendida.
                    var total = profitPerUnit * item.SoldQty;

                    // Agregamos el resultado a la lista.
                    profitList.Add((prod.Name, item.SoldQty, profitPerUnit, total));
                }
            }

            // 7️⃣ Creamos un "ViewModel" para enviar todos los datos a la vista.
            // Este modelo contiene toda la información que se mostrará en el Dashboard.
            var vm = new DashboardViewModel
            {
                TotalProducts = products.Count,        // Total de productos en la base.
                LowStockCount = lowStock.Count,        // Cantidad de productos con stock bajo.
                LowStockProducts = lowStock,           // Lista de productos con stock bajo.
                TopSold = topSoldWithProduct,          // Lista de productos más vendidos.
                Profitability = profitList             // Lista con la rentabilidad de cada producto.
            };

            // 8️⃣ Finalmente devolvemos la vista "Index" junto con el modelo (vm).
            // Esto mostrará los datos en la página principal del Dashboard.
            return View(vm);
        }
    }

    // Este "ViewModel" (modelo de vista) se usa para pasar datos del controlador a la vista.
    // Agrupa toda la información que necesita mostrar el panel principal (Dashboard).
    public class DashboardViewModel
    {
        // Total de productos registrados en la base de datos.
        public int TotalProducts { get; set; }

        // Cantidad de productos que están con stock bajo.
        public int LowStockCount { get; set; }

        // Lista de los productos con bajo stock.
        public List<StockCare.Models.Product> LowStockProducts { get; set; } = new();

        // Lista de productos más vendidos (nombre y cantidad vendida).
        public List<(string Name, int Quantity)> TopSold { get; set; } = new();

        // Lista con la rentabilidad de los productos (nombre, cantidad vendida, ganancia por unidad y total).
        public List<(string Name, int SoldQty, decimal ProfitPerUnit, decimal TotalProfit)> Profitability { get; set; } = new();
    }
}
