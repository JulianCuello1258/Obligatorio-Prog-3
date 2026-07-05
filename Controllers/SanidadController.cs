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

        public async Task<IActionResult> Index(string sortBy = "Fecha", bool descending = true)
        {
            ViewBag.SortBy = sortBy;
            ViewBag.Descending = descending;

            var query = _context.Revisiones
                .Include(r => r.Apiario)
                .Include(r => r.Colmena)
                    .ThenInclude(c => c!.Apiario)
                .AsQueryable();

            switch (sortBy)
            {
                case "Colmena":
                    query = descending ? query.OrderByDescending(r => r.ColmenaId) : query.OrderBy(r => r.ColmenaId);
                    break;
                case "Enfermedad":
                    query = descending ? query.OrderByDescending(r => r.Enfermedades) : query.OrderBy(r => r.Enfermedades);
                    break;
                case "Fecha":
                default:
                    query = descending ? query.OrderByDescending(r => r.Fecha) : query.OrderBy(r => r.Fecha);
                    break;
            }

            var revisiones = await query.ToListAsync();
            return View(revisiones);
        }

        public async Task<IActionResult> Create(int? colmenaId)
        {
            int? apiarioId = null;
            if (colmenaId.HasValue)
            {
                var colmena = await _context.Colmenas.FindAsync(colmenaId.Value);
                if (colmena != null)
                {
                    apiarioId = colmena.ApiarioId;
                }
            }

            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre", apiarioId);
            
            var colmenas = await _context.Colmenas
                .Where(c => c.Estado != EstadoColmena.Perdida)
                .ToListAsync();
            ViewBag.ColmenaId = new SelectList(colmenas, "Id", "Id", colmenaId);

            // Baseline lists of diseases and treatments + dynamically loaded DB distinct items
            var baseEnfermedades = new List<string> { "Ninguna", "Varroasis", "Loque Americana", "Loque Europea", "Nosemosis" };
            var baseTratamientos = new List<string> { "Ninguno", "Ácido Oxálico", "Ácido Fórmico", "Timol", "Amitraz" };

            var dbEnfermedades = await _context.Revisiones
                .Where(r => !string.IsNullOrEmpty(r.Enfermedades))
                .Select(r => r.Enfermedades!)
                .Distinct()
                .ToListAsync();

            var dbTratamientos = await _context.Revisiones
                .Where(r => !string.IsNullOrEmpty(r.Tratamiento))
                .Select(r => r.Tratamiento!)
                .Distinct()
                .ToListAsync();

            ViewBag.Enfermedades = baseEnfermedades.Union(dbEnfermedades).Distinct().ToList();
            ViewBag.Tratamientos = baseTratamientos.Union(dbTratamientos).Distinct().ToList();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetColmenasByApiario(int apiarioId)
        {
            // Exclude Perdida colmenas; return all if apiarioId == 0
            var query = _context.Colmenas
                .Where(c => c.Estado != EstadoColmena.Perdida);

            if (apiarioId > 0)
                query = query.Where(c => c.ApiarioId == apiarioId);

            var colmenas = await query
                .Select(c => new { id = c.Id })
                .ToListAsync();
            return Json(colmenas);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var revision = await _context.Revisiones
                .Include(r => r.Apiario)
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
            if (!revision.ApiarioId.HasValue || revision.ApiarioId <= 0)
            {
                ModelState.AddModelError("ApiarioId", "El apiario es requerido.");
            }

            if (revision.Fecha.Date > DateTime.Today)
            {
                ModelState.AddModelError("Fecha", "La fecha de inspección no puede ser posterior al día de hoy.");
            }

            bool tieneTratamiento = !string.IsNullOrEmpty(revision.Tratamiento) && revision.Tratamiento != "Ninguno";
            if (tieneTratamiento)
            {
                if (!revision.ProximaDosis.HasValue)
                {
                    ModelState.AddModelError("ProximaDosis", "La fecha de la próxima dosis es requerida si se realiza un tratamiento.");
                }
                else if (revision.ProximaDosis.Value.Date <= DateTime.Today)
                {
                    ModelState.AddModelError("ProximaDosis", "La fecha de la próxima dosis debe ser posterior al día de hoy.");
                }
            }
            else
            {
                if (revision.ProximaDosis.HasValue)
                {
                    ModelState.AddModelError("ProximaDosis", "No se puede registrar una próxima dosis sin especificar un tratamiento.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(revision);
                await _context.SaveChangesAsync();

                if (revision.ProximaDosis.HasValue)
                {
                    var tarea = new Tarea
                    {
                        ApiarioId = revision.ApiarioId,
                        ColmenaId = revision.ColmenaId,
                        Titulo = $"Aplicar dosis: {revision.Tratamiento}",
                        Descripcion = $"Segunda dosis: {revision.Tratamiento}. Dosis recomendada: {revision.Dosis}.",
                        FechaProgramada = revision.ProximaDosis.Value,
                        Completada = false
                    };
                    _context.Add(tarea);
                    await _context.SaveChangesAsync();
                }

                TempData["Toast"] = "Inspección sanitaria registrada correctamente.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre");
            
            var colmenas = await _context.Colmenas
                .Where(c => c.Estado != EstadoColmena.Perdida)
                .ToListAsync();
            ViewBag.ColmenaId = new SelectList(colmenas, "Id", "Id", revision.ColmenaId);

            var baseEnfermedades = new List<string> { "Ninguna", "Varroasis", "Loque Americana", "Loque Europea", "Nosemosis" };
            var baseTratamientos = new List<string> { "Ninguno", "Ácido Oxálico", "Ácido Fórmico", "Timol", "Amitraz" };

            var dbEnfermedades = await _context.Revisiones
                .Where(r => !string.IsNullOrEmpty(r.Enfermedades))
                .Select(r => r.Enfermedades!)
                .Distinct()
                .ToListAsync();

            var dbTratamientos = await _context.Revisiones
                .Where(r => !string.IsNullOrEmpty(r.Tratamiento))
                .Select(r => r.Tratamiento!)
                .Distinct()
                .ToListAsync();

            ViewBag.Enfermedades = baseEnfermedades.Union(dbEnfermedades).Distinct().ToList();
            ViewBag.Tratamientos = baseTratamientos.Union(dbTratamientos).Distinct().ToList();

            return View(revision);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var revision = await _context.Revisiones.FindAsync(id);
            if (revision != null)
            {
                _context.Revisiones.Remove(revision);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Inspección eliminada correctamente.";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
