using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Controllers
{
    public class ApiariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApiariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Apiarios
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            var apiarios = from a in _context.Apiarios.Include(a => a.Colmenas)
                           select a;

            apiarios = sortOrder switch
            {
                "name_desc" => apiarios.OrderByDescending(s => s.Nombre),
                "Date" => apiarios.OrderBy(s => s.Id), // Usando Id como proxy de fecha si no hay fecha
                "date_desc" => apiarios.OrderByDescending(s => s.Id),
                _ => apiarios.OrderBy(s => s.Nombre),
            };

            return View(await apiarios.ToListAsync());
        }

        // GET: Apiarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var apiario = await _context.Apiarios
                .Include(a => a.Colmenas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apiario == null) return NotFound();

            return View(apiario);
        }

        // GET: Apiarios/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Latitud,Longitud,Tipo,SeccionPolicial,Zona,TrashumanciaHabilitada,Departamento,Paraje")] Apiario apiario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(apiario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(apiario);
        }

        // GET: Apiarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var apiario = await _context.Apiarios
                .Include(a => a.Colmenas)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiario == null) return NotFound();
            return View(apiario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Latitud,Longitud,Tipo,SeccionPolicial,Zona,TrashumanciaHabilitada,Departamento,Paraje")] Apiario apiario)
        {
            if (id != apiario.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apiario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiarioExists(apiario.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(apiario);
        }

        // GET: Apiarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var apiario = await _context.Apiarios.FirstOrDefaultAsync(m => m.Id == id);
            if (apiario == null) return NotFound();
            return View(apiario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apiario = await _context.Apiarios
                .Include(a => a.Colmenas)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (apiario != null) 
            {
                // Eliminar Tareas asociadas al Apiario
                var tareasApiario = _context.Tareas.Where(t => t.ApiarioId == id);
                _context.Tareas.RemoveRange(tareasApiario);

                // Eliminar Tareas asociadas a las Colmenas del Apiario
                var colmenaIds = apiario.Colmenas.Select(c => c.Id).ToList();
                if (colmenaIds.Any())
                {
                    var tareasColmenas = _context.Tareas.Where(t => t.ColmenaId.HasValue && colmenaIds.Contains(t.ColmenaId.Value));
                    _context.Tareas.RemoveRange(tareasColmenas);
                }

                // Eliminar Trashumancias asociadas
                var trashumancias = _context.Trashumancias.Where(t => t.ApiarioOrigenId == id || t.ApiarioDestinoId == id);
                _context.Trashumancias.RemoveRange(trashumancias);

                _context.Apiarios.Remove(apiario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ApiarioExists(int id)
        {
            return _context.Apiarios.Any(e => e.Id == id);
        }

        // GET: Apiarios/Comparacion
        public async Task<IActionResult> Comparacion()
        {
            var apiarios = await _context.Apiarios
                .Include(a => a.Colmenas)
                    .ThenInclude(c => c.Extracciones)
                .ToListAsync();

            var model = apiarios.Select(a => new ApiarioComparacionViewModel
            {
                Id = a.Id,
                Nombre = a.Nombre,
                TotalColmenas = a.Colmenas.Count,
                ColmenasActivas = a.Colmenas.Count(c => c.Estado == EstadoColmena.Activa),
                ProduccionTotal = a.Colmenas.SelectMany(c => c.Extracciones).Sum(e => e.CantidadKg)
            }).OrderByDescending(x => x.ProduccionTotal).ToList();

            return View(model);
        }
    }

    public class ApiarioComparacionViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TotalColmenas { get; set; }
        public int ColmenasActivas { get; set; }
        public double ProduccionTotal { get; set; }
    }
}
