using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {

        // Initialize Logger, context and UserManager
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}