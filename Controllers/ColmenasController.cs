using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Controllers
{
    public class ColmenasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ColmenasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Colmenas
        public async Task<IActionResult> Index(int? apiarioId)
        {
            var colmenas = _context.Colmenas
                .Include(c => c.Apiario)
                .Include(c => c.Reina)
                .AsQueryable();

            if (apiarioId.HasValue)
            {
                colmenas = colmenas.Where(c => c.ApiarioId == apiarioId);
            }

            return View(await colmenas.ToListAsync());
        }

        // GET: Colmenas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var colmena = await _context.Colmenas
                .Include(c => c.Apiario)
                .Include(c => c.Reina)
                .Include(c => c.Revisiones)
                .Include(c => c.Extracciones)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (colmena == null) return NotFound();

            return View(colmena);
        }

        // GET: Colmenas/Create
        public IActionResult Create()
        {
            ViewData["ApiarioId"] = new SelectList(_context.Apiarios, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApiarioId,Estado,Tipo,Poblacion,Temperamento,Reina")] Colmena colmena)
        {
            if (ModelState.IsValid)
            {
                if (colmena.Reina != null)
                {
                    colmena.Reina.ColmenaId = colmena.Id;
                }
                else
                {
                    colmena.Reina = new Reina { Presencia = true, Salud = SaludReina.Buena };
                }

                _context.Add(colmena);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiarioId"] = new SelectList(_context.Apiarios, "Id", "Nombre", colmena.ApiarioId);
            return View(colmena);
        }

        // GET: Colmenas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var colmena = await _context.Colmenas
                .Include(c => c.Reina)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (colmena == null) return NotFound();

            ViewData["ApiarioId"] = new SelectList(_context.Apiarios, "Id", "Nombre", colmena.ApiarioId);
            return View(colmena);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ApiarioId,Estado,Tipo,Poblacion,Temperamento,FechaCreacion,Reina")] Colmena colmena)
        {
            if (id != colmena.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (colmena.Reina != null)
                    {
                        colmena.Reina.ColmenaId = colmena.Id;
                        var existingReina = await _context.Reinas.FirstOrDefaultAsync(r => r.ColmenaId == id);
                        if (existingReina != null)
                        {
                            _context.Entry(existingReina).CurrentValues.SetValues(colmena.Reina);
                        }
                        else
                        {
                            _context.Add(colmena.Reina);
                        }
                    }

                    _context.Update(colmena);
                    // EF will throw exception if we track existingReina and then try to update colmena which also has Reina
                    // So we must detach existing Reina or just update the colmena directly if it handles the relationship
                    // A safer approach:
                    // We only modify the existing Reina manually, then detach it.
                    // Actually, let's keep it simple:
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Colmenas.Any(e => e.Id == colmena.Id)) return NotFound();
                    else throw;
                }
                // Need to clear tracker for simple update
                _context.ChangeTracker.Clear();
                _context.Update(colmena);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiarioId"] = new SelectList(_context.Apiarios, "Id", "Nombre", colmena.ApiarioId);
            return View(colmena);
        }

        // GET: Colmenas/GestionarReina/5
        public async Task<IActionResult> GestionarReina(int? id)
        {
            if (id == null) return NotFound();

            var colmena = await _context.Colmenas
                .Include(c => c.Reina)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colmena == null) return NotFound();

            if (colmena.Reina == null)
            {
                colmena.Reina = new Reina { ColmenaId = colmena.Id, Presencia = false };
            }

            return View(colmena);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarReina(int id, [Bind("ColmenaId,Salud,Presencia,FechaNacimiento")] Reina reina)
        {
            if (id != reina.ColmenaId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingReina = await _context.Reinas.FirstOrDefaultAsync(r => r.ColmenaId == id);
                    if (existingReina != null)
                    {
                        _context.Entry(existingReina).CurrentValues.SetValues(reina);
                    }
                    else
                    {
                        _context.Add(reina);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Details), new { id = reina.ColmenaId });
            }
            
            var colmena = await _context.Colmenas.FirstOrDefaultAsync(c => c.Id == id);
            if (colmena != null) colmena.Reina = reina;
            
            return View(colmena);
        }
    }
}
