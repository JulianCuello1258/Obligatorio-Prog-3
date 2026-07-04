using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models.Entities;

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
        public async Task<IActionResult> Exportaciones(string sortBy = "Fecha", bool descending = true)
        {
            ViewBag.SortBy = sortBy;
            ViewBag.Descending = descending;

            var query = _context.Exportaciones.AsQueryable();

            switch (sortBy)
            {
                case "Cantidad":
                    query = descending ? query.OrderByDescending(e => e.CantidadBarriles) : query.OrderBy(e => e.CantidadBarriles);
                    break;
                case "Destino":
                    query = descending ? query.OrderByDescending(e => e.Destino) : query.OrderBy(e => e.Destino);
                    break;
                case "Fecha":
                default:
                    query = descending ? query.OrderByDescending(e => e.Fecha) : query.OrderBy(e => e.Fecha);
                    break;
            }

            var exportaciones = await query.ToListAsync();
            return View(exportaciones);
        }

        // GET: Export/CreateExportacion
        public async Task<IActionResult> CreateExportacion()
        {
            var totalKgExtraido = await _context.Extracciones.SumAsync(e => (double?)e.CantidadKg) ?? 0;
            var totalKgExportado = await _context.Exportaciones.SumAsync(e => (double?)e.CantidadBarriles * 300.0) ?? 0;
            var disponibleKg = Math.Max(0, totalKgExtraido - totalKgExportado);
            ViewBag.StockDisponibleKg = disponibleKg;
            return View();
        }

        // POST: Export/CreateExportacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExportacion([Bind("Destino,Fecha")] Exportacion exportacion, double cantidadKg = 0)
        {
            if (exportacion.Fecha.Date > DateTime.Today)
            {
                ModelState.AddModelError("Fecha", "La fecha de exportación no puede ser posterior al día de hoy.");
            }

            // Calcular stock disponible
            var totalKgExtraido = await _context.Extracciones.SumAsync(e => (double?)e.CantidadKg) ?? 0;
            var totalKgExportado = await _context.Exportaciones.SumAsync(e => (double?)e.CantidadBarriles * 300.0) ?? 0;
            var disponibleKg = Math.Max(0, totalKgExtraido - totalKgExportado);

            if (cantidadKg <= 0)
            {
                TempData["Toast"] = "Ingrese una cantidad de kilogramos mayor a 0.";
                TempData["ToastType"] = "danger";
                ViewBag.StockDisponibleKg = disponibleKg;
                ViewBag.CantidadKg = cantidadKg;
                ModelState.AddModelError("cantidadKg", "Ingrese una cantidad de kilogramos mayor a 0.");
                return View(exportacion);
            }

            if (cantidadKg > disponibleKg)
            {
                TempData["Toast"] = $"Stock insuficiente. Tenés disponibles {disponibleKg:N1} kg ({Math.Floor(disponibleKg / 300.0)} barriles completos).";
                TempData["ToastType"] = "danger";
                ViewBag.StockDisponibleKg = disponibleKg;
                ViewBag.CantidadKg = cantidadKg;
                ModelState.AddModelError("cantidadKg", $"Stock insuficiente. Disponibles: {disponibleKg:N1} kg.");
                return View(exportacion);
            }

            // Calcular barriles (300 kg por barril, redondeado hacia abajo)
            exportacion.CantidadBarriles = (int)Math.Floor(cantidadKg / 300.0);
            if (exportacion.CantidadBarriles < 1) exportacion.CantidadBarriles = 1;

            // Validar campos restantes del modelo
            ModelState.Remove("CantidadBarriles");
            if (!ModelState.IsValid)
            {
                ViewBag.StockDisponibleKg = disponibleKg;
                ViewBag.CantidadKg = cantidadKg;
                return View(exportacion);
            }

            _context.Add(exportacion);
            await _context.SaveChangesAsync();
            TempData["Toast"] = $"Exportación de {cantidadKg:N1} kg ({exportacion.CantidadBarriles} barriles) registrada correctamente.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Exportaciones));
        }

        // POST: Export/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var exportacion = await _context.Exportaciones.FindAsync(id);
            if (exportacion != null)
            {
                _context.Exportaciones.Remove(exportacion);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Exportación quitada correctamente.";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction(nameof(Exportaciones));
        }
    }
}
