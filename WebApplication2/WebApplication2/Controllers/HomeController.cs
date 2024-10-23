using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static List<AreaChange> changes = new List<AreaChange>();
        private static List<UserData> users = new List<UserData>();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public ViewResult MapCorrection(AreaChange areaChange)
        {
            if (ModelState.IsValid)
            {
                return View("SubmittedMapErrors", areaChange);
            }
            return View(areaChange);
        }

        // Handle GET request to display the MapCorrection view
        [HttpGet]
        public IActionResult MapCorrection()
        {
            return View();
        }

        // Handle form submission to register a new change
        [HttpPost]
        public IActionResult SubmitMapCorrection(string geoJson, string description, string mapVariant)
        {
            var newChange = new AreaChange
            {
                Id = Guid.NewGuid().ToString(),
                GeoJson = geoJson,
                Description = description,
                MapVariant = mapVariant // save the map variant
            };

            // Save the change in the static in-memory list
            changes.Add(newChange);

            // Redirect to the overview of changes, passing the map variant as a query parameter
            return RedirectToAction("Overview", new { mapVariant });
        }

        // Display the overview of changes
        [HttpGet]
        public IActionResult Overview(string mapVariant)
        {
            var viewModel = new OverviewModel
            {
                AreaChanges = changes,
                UserDatas = users,
                SelectedMapVariant = mapVariant
            };

            return View(viewModel);
        }


        // New action method to display caseworker-specific reports
        [HttpGet]
        public IActionResult CaseworkerOverview()
        {
            return View(changes);
        }


        // Action method to update the status of an report based on its ID
        [HttpPost]
        public IActionResult UpdateStatus(string id, Status status)
        {
            // Finds the first 'areaChange' in 'changes' that matches the given 'id'.
            var areaChange = changes.FirstOrDefault(ac => ac.Id == id);

            // If a change with the specified ID is found, update its status.
            if (areaChange != null)
            {
                areaChange.Status = status;
            }
            return RedirectToAction("CaseworkerOverview");
        }

    }

}