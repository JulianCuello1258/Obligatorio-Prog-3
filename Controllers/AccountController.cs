using Microsoft.AspNetCore.Mvc;

namespace BeeKeeperApp.Controllers
{
    public class AccountController : Controller
    {
        private const string HardcodedPassword = "MatiApicultor";

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string password)
        {
            if (password == HardcodedPassword)
            {
                HttpContext.Session.SetString("IsAuthenticated", "true");
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Contraseña incorrecta";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
