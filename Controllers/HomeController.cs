using EjustLostAndFoundHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EjustLostAndFoundHub.Controllers
{
    public class HomeController : Controller
    {
        // View for the home page
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        // View for the Error 404 page
        public IActionResult Error404()
        {
            return View();
        }

        // View for other Errors page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
