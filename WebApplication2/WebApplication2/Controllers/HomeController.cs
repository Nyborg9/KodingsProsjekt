using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}