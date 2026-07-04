using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Controllers
{
    public class ProduccionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProduccionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortBy = "Fecha", bool descending = true)
        {
            ViewBag.SortBy = sortBy;
            ViewBag.Descending = descending;

            var query = _context.Extracciones
                .Include(e => e.Apiario)
                .Include(e => e.Colmena)
                .ThenInclude(c => c!.Apiario)
                .AsQueryable();

            switch (sortBy)
            {
                case "Cantidad":
                    query = descending ? query.OrderByDescending(e => e.CantidadKg) : query.OrderBy(e => e.CantidadKg);
                    break;
                case "Colmena":
                    query = descending ? query.OrderByDescending(e => e.ColmenaId) : query.OrderBy(e => e.ColmenaId);
                    break;
                case "Fecha":
                default:
                    query = descending ? query.OrderByDescending(e => e.Fecha) : query.OrderBy(e => e.Fecha);
                    break;
            }

            var extracciones = await query.ToListAsync();

            var totalKg = await _context.Extracciones.SumAsync(e => e.CantidadKg);
            var totalKgExportado = await _context.Exportaciones.SumAsync(e => e.CantidadBarriles * 300.0);

            ViewBag.TotalKg = totalKg;
            // disponible = total extraído - total exportado
            ViewBag.TotalDisponible = Math.Max(0.0, totalKg - totalKgExportado);
            ViewBag.TotalBarriles = Math.Floor(ViewBag.TotalDisponible / 300.0); // 300kg per barrel

            return View(extracciones);
        }

        public IActionResult Create()
        {
            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre");
            
            var colmenas = _context.Colmenas
                .Include(c => c.Apiario)
                .Where(c => c.Estado != EstadoColmena.Perdida)
                .ToList();

            ViewBag.ColmenaId = new SelectList(colmenas.Select(c => new {
                Id = c.Id,
                DisplayName = $"Colmena #{c.Id} (Apiario: {c.Apiario?.Nombre ?? "Sin Asignar"})"
            }), "Id", "DisplayName");

            return View(new ExtraccionCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExtraccionCreateViewModel model)
        {
            if (model.TipoRegistro == "Colmena" && !model.ColmenaId.HasValue)
            {
                ModelState.AddModelError("ColmenaId", "Debe seleccionar una colmena.");
            }
            if (model.TipoRegistro == "Apiario" && !model.ApiarioId.HasValue)
            {
                ModelState.AddModelError("ApiarioId", "Debe seleccionar un apiario.");
            }
            if (model.Fecha.Date > DateTime.Today)
            {
                ModelState.AddModelError("Fecha", "La fecha de la cosecha no puede ser posterior al día de hoy.");
            }

            if (ModelState.IsValid)
            {
                if (model.TipoRegistro == "Colmena")
                {
                    var colmena = await _context.Colmenas.FindAsync(model.ColmenaId!.Value);
                    if (colmena == null)
                    {
                        ModelState.AddModelError("ColmenaId", "La colmena seleccionada no existe.");
                    }
                    else if (colmena.Estado == EstadoColmena.Perdida)
                    {
                        ModelState.AddModelError("ColmenaId", "No se puede extraer de una colmena perdida.");
                    }
                    else
                    {
                        var extraccion = new Extraccion
                        {
                            ColmenaId = model.ColmenaId.Value,
                            CantidadKg = model.CantidadKg,
                            Fecha = model.Fecha
                        };

                        _context.Add(extraccion);
                        colmena.ProduccionAcumulada = 0; // Reset to 0
                        _context.Update(colmena);

                        await _context.SaveChangesAsync();
                        TempData["Toast"] = $"Extracción de {model.CantidadKg} kg registrada correctamente para la colmena #{colmena.Id}.";
                        TempData["ToastType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else // Apiario
                {
                    var colmenasActivas = await _context.Colmenas
                        .Where(c => c.ApiarioId == model.ApiarioId!.Value && c.Estado == EstadoColmena.Activa)
                        .ToListAsync();

                    if (!colmenasActivas.Any())
                    {
                        ModelState.AddModelError("ApiarioId", "El apiario no tiene colmenas activas para realizar la extracción.");
                    }
                    else if (model.DistribuirEntreColmenas)
                    {
                        // Distribuir igualitariamente: un registro por colmena activa
                        double cantidadPorColmena = model.CantidadKg / colmenasActivas.Count;
                        foreach (var colmena in colmenasActivas)
                        {
                            _context.Add(new Extraccion
                            {
                                ColmenaId = colmena.Id,
                                CantidadKg = cantidadPorColmena,
                                Fecha = model.Fecha
                            });
                            colmena.ProduccionAcumulada = 0;
                            _context.Update(colmena);
                        }

                        await _context.SaveChangesAsync();
                        TempData["Toast"] = $"Extracción de {model.CantidadKg} kg distribuida igualitariamente entre {colmenasActivas.Count} colmenas activas del apiario ({cantidadPorColmena:N1} kg c/u).";
                        TempData["ToastType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // Registro único agrupado por apiario
                        _context.Add(new Extraccion
                        {
                            ApiarioId = model.ApiarioId!.Value,
                            ColmenaId = null,
                            CantidadKg = model.CantidadKg,
                            Fecha = model.Fecha
                        });

                        foreach (var colmena in colmenasActivas)
                        {
                            colmena.ProduccionAcumulada = 0;
                            _context.Update(colmena);
                        }

                        await _context.SaveChangesAsync();
                        TempData["Toast"] = $"Extracción de {model.CantidadKg} kg registrada correctamente para el apiario ({colmenasActivas.Count} colmenas activas).";
                        TempData["ToastType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            ViewBag.ApiarioId = new SelectList(_context.Apiarios, "Id", "Nombre", model.ApiarioId);
            
            var allColmenas = _context.Colmenas
                .Include(c => c.Apiario)
                .Where(c => c.Estado != EstadoColmena.Perdida)
                .ToList();

            ViewBag.ColmenaId = new SelectList(allColmenas.Select(c => new {
                Id = c.Id,
                DisplayName = $"Colmena #{c.Id} (Apiario: {c.Apiario?.Nombre ?? "Sin Asignar"})"
            }), "Id", "DisplayName", model.ColmenaId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var extraccion = await _context.Extracciones.FindAsync(id);

            if (extraccion != null)
            {
                _context.Extracciones.Remove(extraccion);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Cosecha quitada correctamente.";
                TempData["ToastType"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetColmenasByApiario(int apiarioId)
        {
            // Excluimos las colmenas Perdidas
            var colmenas = await _context.Colmenas
                .Where(c => c.ApiarioId == apiarioId && c.Estado != EstadoColmena.Perdida)
                .Select(c => new { id = c.Id })
                .ToListAsync();
            return Json(colmenas);
        }
    }
}