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

        public async Task<IActionResult> Index()
        {
            var tareas = await _context.Tareas
                .Include(t => t.Colmena)
                .Include(t => t.Apiario)
                .OrderBy(t => t.FechaProgramada)
                .ToListAsync();
            return View(tareas);
        }

        [HttpPost]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                tarea.Completada = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre");
            ViewBag.ColmenaId = new SelectList(_context.Colmenas, "Id", "Id");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tarea tarea)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tarea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre", tarea.ApiarioId);
            ViewBag.ColmenaId = new SelectList(_context.Colmenas, "Id", "Id", tarea.ColmenaId);
            return View(tarea);
        }
        [HttpGet]
        public async Task<IActionResult> GetPendingTasks()
        {
            var pending = await _context.Tareas
                .Where(t => !t.Completada)
                .OrderBy(t => t.FechaProgramada)
                .Take(5)
                .Select(t => new { t.Descripcion, Fecha = t.FechaProgramada.ToShortDateString() })
                .ToListAsync();
            return Json(pending);
        }
    }
}
