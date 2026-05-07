using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Controllers
{
    public class TrashumanciaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrashumanciaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movs = await _context.Trashumancias
                .Include(t => t.ApiarioOrigen)
                .Include(t => t.ApiarioDestino)
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();
            return View(movs);
        }

        public IActionResult Create()
        {
            ViewData["Apiarios"] = new SelectList(_context.Apiarios, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Trashumancia mov)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mov);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Apiarios = new SelectList(_context.Apiarios, "Id", "Nombre", mov.ApiarioOrigenId);
            return View(mov);
        }
    }
}
