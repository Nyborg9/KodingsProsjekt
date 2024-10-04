using Microsoft.AspNetCore.Mvc;

namespace Prosjekt_1.Controllers
{
    public class BrukerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
