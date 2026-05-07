using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;

namespace BeeKeeperApp.Controllers
{
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> DeclaracionJurada()
        {
            var apiarios = await _context.Apiarios.Include(a => a.Colmenas).ToListAsync();
            return View(apiarios);
        }
    }
}
