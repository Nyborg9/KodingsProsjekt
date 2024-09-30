using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static List<AreaChange> AreaChanges = new List<AreaChange>();


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

            /*
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
            */

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
        public ViewResult MapCorrection(AreaChange areaChange)
        {
            if (ModelState.IsValid)
            {
                return View("SubmittedMapErrors", areaChange);
            }
            return View(areaChange);
            
        }

        //handle form submission to register a new change
        [HttpGet]
        public IActionResult RegisterAreaChange()
        {
            return View();
        }

        //handle form submission to register a new change
        [HttpPost]
        public IActionResult RegisterAreaChange(string geoJson, string description)
        {
            var newChange = new AreaChange
            {
                Id = Guid.NewGuid().ToString(),
                GeoJson = geoJson,
                Description = description
            };

            // Save the change in the static in-memory list
            AreaChanges.Add(newChange);

            // Redirect to the overview of changes
            return RedirectToAction("AreaChangeOverview");
        }

        // Display the overview of changes
        [HttpGet]
        public IActionResult AreaChangeOverview()
        {
            return View(AreaChanges);
        }
    }

}
