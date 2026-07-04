using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Controllers
{
    public class TareasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TareasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortBy = "Fecha", bool descending = false)
        {
            ViewBag.SortBy = sortBy;
            ViewBag.Descending = descending;

            var query = _context.Tareas
                .Include(t => t.Colmena)
                .Include(t => t.Apiario)
                .AsQueryable();

            switch (sortBy)
            {
                case "Titulo":
                    query = descending ? query.OrderByDescending(t => t.Titulo) : query.OrderBy(t => t.Titulo);
                    break;
                case "Estado":
                    query = descending ? query.OrderByDescending(t => t.Completada) : query.OrderBy(t => t.Completada);
                    break;
                case "Fecha":
                default:
                    query = descending ? query.OrderByDescending(t => t.FechaProgramada) : query.OrderBy(t => t.FechaProgramada);
                    break;
            }

            var tareas = await query.ToListAsync();
            return View(tareas);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tarea = await _context.Tareas
                .Include(t => t.Colmena)
                .Include(t => t.Apiario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tarea == null) return NotFound();

            return View(tarea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                tarea.Completada = true;
                await _context.SaveChangesAsync();
                TempData["Toast"] = $"Tarea marcada como completada.";
                TempData["ToastType"] = "success";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                _context.Tareas.Remove(tarea);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Tarea eliminada correctamente.";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre");
            ViewBag.ColmenaId = new SelectList(_context.Colmenas.Where(c => c.Estado != EstadoColmena.Perdida), "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tarea tarea)
        {
            if (tarea.FechaProgramada < DateTime.Today)
            {
                ModelState.AddModelError("FechaProgramada", "La fecha programada debe ser posterior o igual al día de hoy.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(tarea);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Tarea programada correctamente.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre", tarea.ApiarioId);
            ViewBag.ColmenaId = new SelectList(_context.Colmenas.Where(c => c.Estado != EstadoColmena.Perdida), "Id", "Id", tarea.ColmenaId);
            return View(tarea);
        }

        [HttpGet]
        public async Task<JsonResult> GetColmenasByApiario(int apiarioId)
        {
            var colmenas = await _context.Colmenas
                .Where(c => c.ApiarioId == apiarioId && c.Estado != EstadoColmena.Perdida)
                .Select(c => new { id = c.Id })
                .ToListAsync();
            return Json(colmenas);
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingTasks()
        {
            var pending = await _context.Tareas
                .Where(t => !t.Completada)
                .OrderBy(t => t.FechaProgramada)
                .Take(5)
                .Select(t => new { t.Id, t.Titulo, t.Descripcion, Fecha = t.FechaProgramada.ToShortDateString() })
                .ToListAsync();
            return Json(pending);
        }
    }
}
