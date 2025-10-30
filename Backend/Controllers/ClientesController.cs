using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly StockCareContext _context;

        public ClientesController(StockCareContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Clientes.ToListAsync();
            return Ok(clientes);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarCliente([FromBody] Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return Ok(cliente);
        }
    }
}