using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;
using WebApplication2.Data;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;

        private static List<AreaChange> changes = new List<AreaChange>();
        private static List<UserData> users = new List<UserData>();

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Action method to handle GET request and show RegistrationForm.cshtml view
        [HttpGet]
        public ViewResult RegistrationForm()
        {
            return View();
        }

        // Action method to handle POST request and receive data
        [HttpPost]
        public IActionResult RegistrationForm(UserData userData)
        {
            if (ModelState.IsValid)
            {
                users.Add(userData);
                return RedirectToAction("MapCorrection");
            }
            return View(userData);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ViewResult MapCorrection(GeoChange geoChange)
        {
            if (ModelState.IsValid)
            {
                return View("SubmittedMapErrors", geoChange);
            }
            return View(geoChange);
        }

        // Handle GET request to display the MapCorrection view
        [HttpGet]
        public IActionResult MapCorrection()
        {
            return View();
        }

        // Handle form submission to register a new change
        [HttpPost]
        public IActionResult SubmitMapCorrection(string geoJson, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(geoJson) || string.IsNullOrEmpty(description))
                {
                    return BadRequest("GeoJson and description must be provided");
                }

                var newChange = new GeoChange
                {
                    GeoJson = geoJson,
                    Description = description
                };

                _context.GeoChanges.Add(newChange);
                _context.SaveChanges();

                // Redirect to the overview of changes
                return RedirectToAction("Overview");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}, Inner Exeption; {ex.InnerException?.Message}");
                throw;
            }
        }


        // Display the overview of changes
        [HttpGet]
        public IActionResult Overview()
        {
            var changes_db = _context.GeoChanges.ToList();
            return View(changes_db);
        }
    }
}

