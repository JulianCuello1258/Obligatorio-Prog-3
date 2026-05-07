using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;

namespace BeeKeeperApp.Controllers
{
    public class MapaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MapaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var apiarios = await _context.Apiarios.ToListAsync();
            return View(apiarios);
        }
    }
}
