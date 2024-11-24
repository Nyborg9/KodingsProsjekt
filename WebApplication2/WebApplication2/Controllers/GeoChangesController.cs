using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles="User")] // Applies authorization to the entire controller
    public class GeoChangesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GeoChangesController> _logger;

        public GeoChangesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<GeoChangesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Create(string geoJson, string description, string MapVariant)
        {
            try
            {
                if (string.IsNullOrEmpty(geoJson) || string.IsNullOrEmpty(description))
                {
                    return BadRequest("GeoJson and description must be provided");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found");
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
                    CountyName = countyName,
                    MapVariant = MapVariant
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var geoChange = await _context.GeoChanges
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (geoChange == null)
            {
                return NotFound();
            }

            return View(geoChange);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Description")] GeoChange geoChange)
        {
            try
            {
                // Fetch the existing entity
                var existingGeoChange = await _context.GeoChanges
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (existingGeoChange == null)
                {
                    return NotFound();
                }

                // Only update the description
                existingGeoChange.Description = geoChange.Description;

                // Use Entry to modify only the description
                _context.Entry(existingGeoChange).Property(x => x.Description).IsModified = true;

                await _context.SaveChangesAsync();

                return RedirectToAction("Index","GeoChanges");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error updating GeoChange: {ex.Message}");
                return View(geoChange);
            }
        }

        public async Task<IActionResult> Delete(int? id)
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

            return View(geoChange);
        }

        // POST: GeoChanges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var geoChange = await _context.GeoChanges.FindAsync(id);
            if (geoChange != null)
            {
                _context.GeoChanges.Remove(geoChange);
                await _context.SaveChangesAsync();
            }

            // Redirect to the GeoChange index
            return RedirectToAction("Index", "GeoChanges");
        }

        // Update the exists check to be async
        private async Task<bool> GeoChangeExists(int id)
        {
            return await _context.GeoChanges.AnyAsync(e => e.Id == id);
        }
    }
}