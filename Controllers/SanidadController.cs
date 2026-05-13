using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Controllers
{
    public class SanidadController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SanidadController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var revisiones = await _context.Revisiones
                .Include(r => r.Colmena)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
            return View(revisiones);
        }

        public IActionResult Create(int? colmenaId)
        {
            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre");
            ViewBag.ColmenaId = new SelectList(_context.Colmenas, "Id", "Id", colmenaId);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetColmenasByApiario(int apiarioId)
        {
            var colmenas = await _context.Colmenas
                .Where(c => c.ApiarioId == apiarioId)
                .Select(c => new { id = c.Id })
                .ToListAsync();
            return Json(colmenas);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var revision = await _context.Revisiones
                .Include(r => r.Colmena)
                .ThenInclude(c => c!.Apiario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (revision == null) return NotFound();

            return View(revision);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Revision revision)
        {
            if (revision.ColmenaId <= 0)
            {
                ModelState.AddModelError("ColmenaId", "Debe seleccionar una colmena válida.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(revision);
                await _context.SaveChangesAsync();

                if (revision.ProximaDosis.HasValue)
                {
                    var tarea = new Tarea
                    {
                        ColmenaId = revision.ColmenaId,
                        Descripcion = $"Segunda dosis: {revision.Tratamiento}",
                        FechaProgramada = revision.ProximaDosis.Value,
                        Completada = false
                    };
                    _context.Add(tarea);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.ColmenaId = new SelectList(_context.Colmenas, "Id", "Id", revision.ColmenaId);
            return View(revision);
        }
    }
}
