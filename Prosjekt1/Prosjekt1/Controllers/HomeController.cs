using Microsoft.AspNetCore.Mvc;
using Prosjekt_1.Models;
using System.Diagnostics;

namespace Prosjekt_1.Controllers
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

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RegistrationForm()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistrationForm(UserData model)
        {
            if (ModelState.IsValid)
            {
                // Redirect to the Overview page if the model is valid
                return RedirectToAction("Reports");
            }

            // If the model is not valid, return the same view with validation messages
            return View(model);
        }

        public IActionResult Reports()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Reports(ReportData model)
        {
            if (ModelState.IsValid)
            {
                // Redirect to the Overview page if the model is valid
                return RedirectToAction("Index");
            }

            // If the model is not valid, return the same view with validation messages
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}