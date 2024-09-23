using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // action metode som håndterer GET forespørsel og viser RegistrationForm.cshtml view
        [HttpGet]
        public ViewResult RegistrationForm()
        {
            return View();
        }

        // action metode som håndterer POST forespørsel og mottar data
        [HttpPost]
        public ViewResult RegistrationForm(UserData userData)
        {
            return View("Overview", userData);
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

        public IActionResult MapCorrection()
        {
            return View();
        }
        [HttpPost]
        public ViewResult MapCorrection(UserData userData)
        {
            if (ModelState.IsValid)
            {
                return View("SubmittedMapErrors", userData);
            }
            return View(userData);
            
        }
    }
}
