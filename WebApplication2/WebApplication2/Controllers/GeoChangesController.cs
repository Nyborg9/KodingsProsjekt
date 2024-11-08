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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; 
        // Adds UserManager and ApplicationDbContext

        public GeoChangesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
            // Initializes UserManager and Context
        }

        public async Task<IActionResult> Index()
        {
            var geoChanges = await _context.GeoChanges.ToListAsync(); // Fetch the list of GeoChange objects
            return View(geoChanges); // Return the Index view with the list of GeoChange objects
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
            try
            {
                if (string.IsNullOrEmpty(geoJson) || string.IsNullOrEmpty(description))
                {
                    return BadRequest("GeoJson and description must be provided");
                }

                // Retrieve the UserId from the claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Using ClaimTypes.NameIdentifier to get the UserId

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User  not found");
                }

                var newChange = new GeoChange
                {
                    GeoJson = geoJson,
                    Description = description,
                    UserId = userId // Set the UserId
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


        // Get the Edit form
        [HttpGet]
        public IActionResult Edit(int id, string returnUrl = null)
        {
            var geoChange = _context.GeoChanges.Find(id);
            if (geoChange == null)
            {
                return NotFound();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(geoChange);
        }

        // Edit Action (POST) to update a GeoChange
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
                return RedirectToAction("Index");
            }

            return View(geoChange); // Return the view with the current geoChange if model state is invalid
        }
  
    // GET: GeoChanges/Delete/5
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

            return View("Delete", geoChange);
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GeoChangeExists(int id)
        {
            return _context.GeoChanges.Any(e => e.Id == id);
        }
    }
}