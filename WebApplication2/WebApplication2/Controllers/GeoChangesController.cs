﻿using System;
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

            return View(geoChange); // Ensure you return the model for editing
        }

        // POST: GeoChanges/Edit/5
        // Saves the changes in the database, but does not function correctly as something goes wrong with the modelstate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,GeoJson")] GeoChange geoChange, string returnUrl)
        {
            if (id != geoChange.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(geoChange);  // Marks the entity as modified
                    await _context.SaveChangesAsync();  // Save the changes to the database
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.GeoChanges.Any(e => e.Id == geoChange.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // After successfully updating, return to the page specified by returnUrl
                return Redirect(returnUrl ?? "/Home/Index"); // Default to /Home/Index if no returnUrl is provided
            }

            return View(geoChange);  // Return to the edit view if validation fails
        }

   // GET: GeoChanges/Delete/5
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

        // action metode for å vise en oversikt alle innsendte kartredigeringsforespørsler
        [HttpGet]
        public IActionResult CaseworkerOverview()
        {
            var changes_db = _context.GeoChanges.ToList();
            if (changes_db == null || !changes_db.Any())
            {
                return View("~/Views/Caseworker/CaseworkerOverview.cshtml", new List<GeoChange>()); // Specify the view name
            }
            return View("~/Views/Caseworker/CaseworkerOverview.cshtml", changes_db); // Specify the view name
        }
        public IActionResult Details(int id)
        {
            var report = _context.GeoChanges.Find(id); // Fetch the specific report by ID
            if (report == null)
            {
                return NotFound();
            }
            return View("~/Views/Caseworker/ReportDetails.cshtml", report);
        }
    }
}