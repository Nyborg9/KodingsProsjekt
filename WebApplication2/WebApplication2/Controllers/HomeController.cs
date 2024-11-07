using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using System.Diagnostics;
using WebApplication2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;


        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        // Display the overview of changes
        [HttpGet]
        public IActionResult Overview()
        {
            var changes_db = _context.GeoChanges.ToList();
            return View(changes_db);
        }

        [Authorize]
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
    }
}