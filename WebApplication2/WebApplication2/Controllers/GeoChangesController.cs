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

        public GeoChangesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Initializes UserManager and Context
            _context = context;
            _userManager = userManager;
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
        public IActionResult Create(string geoJson, string description, string mapVariant)
        {
            //Checks if valid input is given
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

                //Defines a new GeoChange and adds it to the database
                var newChange = new GeoChange
                {
                    GeoJson = geoJson,
                    Description = description,
                    UserId = userId 
                };

                _context.GeoChanges.Add(newChange);
                _context.SaveChanges();

                // Redirect to the overview of changes
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}, Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }

       
        // Henter og viser redigeringsskjemaet
        // GET: GeoChanges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var geoChange = await _context.GeoChanges.FindAsync(id);
            if (geoChange == null)
            {
                return NotFound();
            }

            return View(geoChange);
        }

        // Edit Action (POST) to update a GeoChange currently not working
        [HttpPost]
        public IActionResult Edit(GeoChange geoChange, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                _context.GeoChanges.Update(geoChange); // Update the entity
                _context.SaveChanges(); // Save changes to the database
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
              
            // After successfully updating, return to the page specified by returnUrl
            return Redirect(returnUrl ?? "/Home/Index"); // Default to /Home/Index if no returnUrl is provided
            }

            return View(geoChange);  // Return to the edit view if validation fails
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
            return Redirect(returnUrl ?? Url.Action("Index"));
        }

        private bool GeoChangeExists(int id)
        {
            return _context.GeoChanges.Any(e => e.Id == id);
        }
    }
}