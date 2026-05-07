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
        public async Task<IActionResult> Create([Bind("ApiarioId,Estado,Tipo,Poblacion,Temperamento")] Colmena colmena)
        {
            if (ModelState.IsValid)
            {
                _context.Add(colmena);
                await _context.SaveChangesAsync();

                // Create initial Queen
                var reina = new Reina { ColmenaId = colmena.Id, Presencia = true, Salud = "Buena" };
                _context.Add(reina);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiarioId"] = new SelectList(_context.Apiarios, "Id", "Nombre", colmena.ApiarioId);
            return View(colmena);
        }

        // Actions for Health/Revision and Production will be added to respective controllers or here
    }
}
