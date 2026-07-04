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

        public async Task<IActionResult> Index(string sortBy = "Fecha", bool descending = true)
        {
            ViewBag.SortBy = sortBy;
            ViewBag.Descending = descending;

            var query = _context.Trashumancias
                .Include(t => t.ApiarioOrigen)
                .Include(t => t.ApiarioDestino)
                .Include(t => t.Colmena)
                .AsQueryable();

            switch (sortBy)
            {
                case "Distancia":
                    query = descending ? query.OrderByDescending(t => t.DistanciaKm) : query.OrderBy(t => t.DistanciaKm);
                    break;
                case "Fecha":
                default:
                    query = descending ? query.OrderByDescending(t => t.Fecha) : query.OrderBy(t => t.Fecha);
                    break;
            }

            var movs = await query.ToListAsync();
            return View(movs);
        }

        public IActionResult Create()
        {
            ViewData["Apiarios"] = new SelectList(_context.Apiarios, "Id", "Nombre");
            ViewData["Colmenas"] = new SelectList(Enumerable.Empty<SelectListItem>());
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetApiarios()
        {
            var apiarios = await _context.Apiarios
                .Select(a => new { a.Id, a.Nombre, a.Latitud, a.Longitud })
                .ToListAsync();
            return Json(apiarios);
        }

        [HttpGet]
        public async Task<JsonResult> GetColmenasByApiario(int apiarioId)
        {
            // Exclude Perdida colmenas
            var colmenas = await _context.Colmenas
                .Where(c => c.ApiarioId == apiarioId && c.Estado != EstadoColmena.Perdida)
                .Select(c => new { id = c.Id })
                .ToListAsync();
            return Json(colmenas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trashumancia mov)
        {
            if (mov.Fecha.Date > DateTime.Today)
            {
                ModelState.AddModelError("Fecha", "La fecha del traslado no puede ser posterior al día de hoy.");
            }

            if (mov.ApiarioOrigenId == mov.ApiarioDestinoId)
            {
                ModelState.AddModelError("ApiarioDestinoId", "El apiario de destino debe ser diferente al de origen.");
            }

            var colmena = await _context.Colmenas.FindAsync(mov.ColmenaId);
            if (colmena == null)
            {
                ModelState.AddModelError("ColmenaId", "Debe seleccionar una colmena válida.");
            }
            else if (colmena.ApiarioId != mov.ApiarioOrigenId)
            {
                ModelState.AddModelError("ColmenaId", "La colmena seleccionada no pertenece al apiario de origen.");
            }

            if (ModelState.IsValid)
            {
                // Update colmena's apiario immediately to the destination apiario
                if (colmena != null)
                {
                    colmena.ApiarioId = mov.ApiarioDestinoId;
                    _context.Update(colmena);
                }

                _context.Add(mov);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Movimiento de trashumancia registrado correctamente y colmena trasladada.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Apiarios"] = new SelectList(_context.Apiarios, "Id", "Nombre", mov.ApiarioOrigenId);
            
            var colmenas = await _context.Colmenas
                .Where(c => c.ApiarioId == mov.ApiarioOrigenId && c.Estado != EstadoColmena.Perdida)
                .ToListAsync();
                
            ViewData["Colmenas"] = new SelectList(colmenas.Select(c => new {
                Id = c.Id,
                DisplayName = $"Colmena #{c.Id}"
            }), "Id", "DisplayName", mov.ColmenaId);

            return View(mov);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var mov = await _context.Trashumancias.FindAsync(id);
            if (mov != null)
            {
                _context.Trashumancias.Remove(mov);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Traslado quitado correctamente.";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
