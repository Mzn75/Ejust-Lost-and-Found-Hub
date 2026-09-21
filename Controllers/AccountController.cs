using Microsoft.AspNetCore.Mvc;

namespace EjustLostAndFoundHub.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
