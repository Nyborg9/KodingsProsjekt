
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize] //Applyies authorization to the entire controller
    public class GeoChangesController : Controller
    {
        // Adds UserManager and ApplicationDbContext
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GeoChangesController> _logger;

        public GeoChangesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<GeoChangesController> logger)
        {
            // Initializes UserManager and Context
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        //Shows all of the users reports
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Fetch only the GeoChange records for the logged-in user
            var userChanges = await _context.GeoChanges
                .Where(change => change.UserId == user.Id)
                .ToListAsync();

            return View(userChanges);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: GeoChanges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string geoJson, string description, string mapVariant)
        {
            // Checks if valid input is given
            try
            {
                if (string.IsNullOrEmpty(geoJson) || string.IsNullOrEmpty(description))
                {
                    return BadRequest("GeoJson and description must be provided");
                }

                // Retrieve the UserId from the claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User  not found");
                }

                // Find municipality information
                var (municipalityNumber, municipalityName, countyName) = await FindMunicipalityAsync(geoJson);

                // Log the municipality information
                _logger.LogInformation($"Municipality Number: {municipalityNumber}, Municipality Name: {municipalityName}");

                // Check for null values
                if (string.IsNullOrEmpty(municipalityNumber) || string.IsNullOrEmpty(municipalityName))
                {
                    return BadRequest("Could not retrieve municipality information. Please check the coordinates.");
                }

                // Defines a new GeoChange and adds it to the database
                var newChange = new GeoChange
                {
                    GeoJson = geoJson,
                    Description = description,
                    UserId = userId,
                    Status = ReportStatus.IkkePåbegynt,
                    MunicipalityNumber = municipalityNumber,
                    MunicipalityName = municipalityName,
                    CountyName = countyName
                };

                _context.GeoChanges.Add(newChange);
                await _context.SaveChangesAsync();

                // Redirect to the overview of changes
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a GeoChange");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<(string MunicipalityNumber, string MunicipalityName, string CountyName)> FindMunicipalityAsync(string geoJson)
        {
            var municipalityFinderService = HttpContext.RequestServices.GetRequiredService<MunicipalityFinderService>();

            // Explicitly return all three elements
            var result = await municipalityFinderService.FindMunicipalityFromGeoJsonAsync(geoJson);
            return (result.MunicipalityNumber, result.MunicipalityName, result.CountyName);
        }


        // Henter og viser redigeringsskjemaet
        // GET: GeoChanges/Edit/5
        public async Task<IActionResult> Edit(int? id, string returnUrl)
        {
            if (id == null)
            {
                // Return NotFound if no ID is provided
                return NotFound();
            }

            // Fetch the GeoChange entity from the database
            var geoChange = await _context.GeoChanges
                .Include(g => g.User)  // Include the User related to the GeoChange
                .FirstOrDefaultAsync(m => m.Id == id);

            if (geoChange == null)
            {
                // Return NotFound if the GeoChange is not found
                return NotFound();
            }

            // Store the returnUrl in ViewBag for redirection after the form is submitte
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index"); //default to index
            return View(geoChange);
        }

            // Edit Action (POST) to update a GeoChange
            // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,GeoJson,UserId")] GeoChange geoChange, string returnUrl)
        {
            if (id != geoChange.Id)
            {
                return NotFound();
            }

            //remove User and GeoJson from ModelState as only description should be updated
            ModelState.Remove("User");
            ModelState.Remove("GeoJson");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing entity from database
                    var existingGeoChange = await _context.GeoChanges
                        .FirstOrDefaultAsync(g => g.Id == id);

                    if (existingGeoChange == null)
                    {
                        return NotFound();
                    }

                    // the properties that should be updated, here description
                    existingGeoChange.Description = geoChange.Description;


                    _context.Update(existingGeoChange);
                    await _context.SaveChangesAsync();

                    // Use returnUrl if provided, otherwise fall back to Index
                    return Redirect(returnUrl ?? Url.Action("Index"));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.GeoChanges.AnyAsync(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Repopulate returnUrl in case of validation failure
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index");
            return View(geoChange);
        }

        // GET: GeoChanges/Delete/
        public async Task<IActionResult> Delete(int? id, string returnUrl)
        { 
            if (id == null)
            {
                return NotFound();
            }

            var geoChange = await _context.GeoChanges
                .FirstOrDefaultAsync(m => m.Id == id);
            if (geoChange == null)
            {
                return NotFound();
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(geoChange);
        }



        // POST: GeoChanges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string returnUrl)
        {
            var geoChange = await _context.GeoChanges.FindAsync(id);
            if (geoChange != null)
            {
                _context.GeoChanges.Remove(geoChange);
                await _context.SaveChangesAsync();
            }

            // Redirect to the URL provided in returnUrl or default to Index if no returnUrl
            return string.IsNullOrEmpty(returnUrl)
                ? RedirectToAction("Index") // Use RedirectToAction here
                : Redirect(returnUrl);
        }

        // Update the exists check to be async
        private async Task<bool> GeoChangeExists(int id)
        {
            return await _context.GeoChanges.AnyAsync(e => e.Id == id);
        }
    }
}