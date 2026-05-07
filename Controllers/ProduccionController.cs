using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Controllers
{
    public class ProduccionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProduccionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var extracciones = await _context.Extracciones
                .Include(e => e.Colmena)
                .ThenInclude(c => c.Apiario)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();

            var totalKg = extracciones.Sum(e => e.CantidadKg);
            ViewBag.TotalKg = totalKg;
            ViewBag.TotalBarriles = Math.Floor(totalKg / 300.0); // 300kg per barrel

            return View(extracciones);
        }

        public IActionResult Create()
        {
            ViewBag.ColmenaId = new SelectList(_context.Colmenas, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Extraccion extraccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(extraccion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ColmenaId = new SelectList(_context.Colmenas, "Id", "Id", extraccion.ColmenaId);
            return View(extraccion);
        }
    }
}