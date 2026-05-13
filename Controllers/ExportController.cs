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

        // GET: Export/Exportaciones
        public async Task<IActionResult> Exportaciones()
        {
            var exportaciones = await _context.Exportaciones.OrderByDescending(e => e.Fecha).ToListAsync();
            return View(exportaciones);
        }

        // GET: Export/CreateExportacion
        public IActionResult CreateExportacion()
        {
            return View();
        }

        // POST: Export/CreateExportacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExportacion([Bind("CantidadBarriles,Destino,Fecha")] BeeKeeperApp.Models.Entities.Exportacion exportacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(exportacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Exportaciones));
            }
            return View(exportacion);
        }
    }
}
