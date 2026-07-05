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
        public async Task<IActionResult> Index(int? apiarioId, string sortBy = "Id", bool descending = false)
        {
            ViewBag.SortBy = sortBy;
            ViewBag.Descending = descending;
            ViewBag.SelectedApiarioId = apiarioId;

            var colmenas = _context.Colmenas
                .Include(c => c.Apiario)
                .Include(c => c.Reina)
                .Include(c => c.Extracciones)
                .AsQueryable();

            if (apiarioId.HasValue)
            {
                colmenas = colmenas.Where(c => c.ApiarioId == apiarioId);
            }

            switch (sortBy)
            {
                case "Fecha":
                    colmenas = descending ? colmenas.OrderByDescending(c => c.FechaCreacion) : colmenas.OrderBy(c => c.FechaCreacion);
                    break;
                case "Produccion":
                    colmenas = descending ? colmenas.OrderByDescending(c => c.Extracciones.Sum(e => e.CantidadKg)) : colmenas.OrderBy(c => c.Extracciones.Sum(e => e.CantidadKg));
                    break;
                case "Poblacion":
                    // Custom sort: Fuerte(3) > Media(2) > Debil(1)
                    colmenas = descending
                        ? colmenas.OrderByDescending(c => c.Poblacion == NivelPoblacion.Fuerte ? 3 : c.Poblacion == NivelPoblacion.Media ? 2 : 1)
                        : colmenas.OrderBy(c => c.Poblacion == NivelPoblacion.Fuerte ? 3 : c.Poblacion == NivelPoblacion.Media ? 2 : 1);
                    break;
                case "Tipo":
                    colmenas = descending ? colmenas.OrderByDescending(c => c.Tipo) : colmenas.OrderBy(c => c.Tipo);
                    break;
                case "Id":
                default:
                    colmenas = descending ? colmenas.OrderByDescending(c => c.Id) : colmenas.OrderBy(c => c.Id);
                    break;
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
        public async Task<IActionResult> Create([Bind("ApiarioId,Estado,Tipo,Poblacion,Temperamento,Reina")] Colmena colmena, int cantidad = 1)
        {
            if (colmena.ApiarioId <= 0)
            {
                ModelState.AddModelError("ApiarioId", "El apiario es requerido.");
            }
            if (string.IsNullOrEmpty(colmena.Tipo))
            {
                ModelState.AddModelError("Tipo", "El tipo de colmena es requerido.");
            }
            if (colmena.Poblacion == null)
            {
                ModelState.AddModelError("Poblacion", "El nivel de población es requerido.");
            }
            if (colmena.Temperamento == null)
            {
                ModelState.AddModelError("Temperamento", "El temperamento es requerido.");
            }
            if (colmena.Estado == EstadoColmena.Perdida)
            {
                ModelState.AddModelError("Estado", "El estado inicial debe ser activa o inactiva.");
            }
            if (colmena.Reina == null)
            {
                ModelState.AddModelError("Reina.Presencia", "Los datos de la reina son requeridos.");
            }
            else
            {
                if (colmena.Reina.Presencia)
                {
                    if (colmena.Reina.FechaNacimiento == null)
                    {
                        ModelState.AddModelError("Reina.FechaNacimiento", "La fecha de nacimiento de la reina es requerida.");
                    }
                    else if (colmena.Reina.FechaNacimiento.Value.Date > DateTime.Today)
                    {
                        ModelState.AddModelError("Reina.FechaNacimiento", "La fecha de nacimiento no puede ser posterior al día de hoy.");
                    }
                }
            }

            if (cantidad < 1) cantidad = 1;
            if (cantidad > 500) cantidad = 500;

            if (ModelState.IsValid)
            {
                // Prepare the queen data from the submitted colmena
                bool reinaPresente = colmena.Reina?.Presencia ?? false;
                SaludReina saludReina = colmena.Reina?.Salud ?? SaludReina.Buena;
                DateTime? fechaNacReina = reinaPresente ? colmena.Reina!.FechaNacimiento : null;

                for (int i = 0; i < cantidad; i++)
                {
                    var nuevaColmena = new Colmena
                    {
                        ApiarioId   = colmena.ApiarioId,
                        Estado      = colmena.Estado,
                        Tipo        = colmena.Tipo,
                        Poblacion   = colmena.Poblacion,
                        Temperamento = colmena.Temperamento,
                        FechaCreacion = DateTime.UtcNow,
                        Reina = new Reina
                        {
                            Presencia       = reinaPresente,
                            Salud           = saludReina,
                            FechaNacimiento = fechaNacReina
                        }
                    };
                    _context.Add(nuevaColmena);
                }

                await _context.SaveChangesAsync();

                TempData["Toast"] = cantidad == 1
                    ? "Colmena registrada correctamente."
                    : $"{cantidad} colmenas registradas correctamente.";
                TempData["ToastType"] = "success";
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

            if (colmena.Estado != EstadoColmena.Activa)
            {
                TempData["Toast"] = "No se puede editar una colmena que no está activa.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Details), new { id = colmena.Id });
            }

            ViewData["ApiarioId"] = new SelectList(_context.Apiarios, "Id", "Nombre", colmena.ApiarioId);
            return View(colmena);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ApiarioId,Estado,Tipo,Poblacion,Temperamento,FechaCreacion,Reina")] Colmena colmena)
        {
            if (id != colmena.Id) return NotFound();

            var existingColmena = await _context.Colmenas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingColmena == null) return NotFound();
            if (existingColmena.Estado != EstadoColmena.Activa)
            {
                TempData["Toast"] = "No se puede editar una colmena que no está activa.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (colmena.Reina != null && colmena.Reina.Presencia)
            {
                if (colmena.Reina.FechaNacimiento != null && colmena.Reina.FechaNacimiento.Value.Date > DateTime.Today)
                {
                    ModelState.AddModelError("Reina.FechaNacimiento", "La fecha de nacimiento no puede ser posterior al día de hoy.");
                }
            }

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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Colmenas.Any(e => e.Id == colmena.Id)) return NotFound();
                    else throw;
                }

                _context.ChangeTracker.Clear();
                _context.Update(colmena);
                await _context.SaveChangesAsync();
                TempData["Toast"] = $"Colmena #{colmena.Id} actualizada correctamente.";
                TempData["ToastType"] = "info";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApiarioId"] = new SelectList(_context.Apiarios, "Id", "Nombre", colmena.ApiarioId);
            return View(colmena);
        }

        // GET: Colmenas/DarDeBaja/5
        public async Task<IActionResult> DarDeBaja(int? id)
        {
            if (id == null) return NotFound();

            var colmena = await _context.Colmenas
                .Include(c => c.Apiario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colmena == null) return NotFound();

            // Capture where the user came from so we can return them there after
            var referer = Request.Headers["Referer"].ToString();
            ViewBag.ReturnUrl = string.IsNullOrEmpty(referer) ? Url.Action("Index") : referer;

            return View(colmena);
        }

        // POST: Colmenas/DarDeBaja/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DarDeBaja(int id, EstadoColmena nuevoEstado, string? returnUrl)
        {
            var colmena = await _context.Colmenas.FindAsync(id);
            if (colmena == null) return NotFound();

            colmena.Estado = EstadoColmena.Perdida;
            _context.Update(colmena);
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"Colmena #{id} eliminada correctamente.";
            TempData["ToastType"] = "warning";

            // Return to where the user came from, or fall back to the hive list
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // GET: Colmenas/GestionarReina/5
        public async Task<IActionResult> GestionarReina(int? id)
        {
            if (id == null) return NotFound();

            var colmena = await _context.Colmenas
                .Include(c => c.Reina)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colmena == null) return NotFound();

            if (colmena.Estado != EstadoColmena.Activa)
            {
                TempData["Toast"] = "No se puede gestionar la reina de una colmena que no está activa.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Details), new { id = colmena.Id });
            }

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

            var colmena = await _context.Colmenas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (colmena == null) return NotFound();
            if (colmena.Estado != EstadoColmena.Activa)
            {
                TempData["Toast"] = "No se puede gestionar la reina de una colmena que no está activa.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (reina.Presencia)
            {
                if (reina.FechaNacimiento == null)
                {
                    ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento de la reina es requerida.");
                }
                else if (reina.FechaNacimiento.Value.Date > DateTime.Today)
                {
                    ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser posterior al día de hoy.");
                }
            }

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
                TempData["Toast"] = "Datos de reina actualizados correctamente.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Details), new { id = reina.ColmenaId });
            }
            
            colmena.Reina = reina;
            return View(colmena);
        }
    }
}
