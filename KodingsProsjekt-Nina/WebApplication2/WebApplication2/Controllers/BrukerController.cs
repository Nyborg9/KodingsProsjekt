using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class BrukerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
