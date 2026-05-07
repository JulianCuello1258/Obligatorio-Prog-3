using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Models;
using BeeKeeperApp.Data;

namespace BeeKeeperApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var stats = new DashboardViewModel
        {
            TotalApiarios = await _context.Apiarios.CountAsync(),
            TotalColmenasActivas = await _context.Colmenas.CountAsync(c => c.Estado == Models.Entities.EstadoColmena.Activa),
            TareasPendientes = await _context.Tareas.Where(t => !t.Completada).CountAsync(),
            UltimasRevisiones = await _context.Revisiones.Include(r => r.Colmena).OrderByDescending(r => r.Fecha).Take(5).ToListAsync()
        };
        return View(stats);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
