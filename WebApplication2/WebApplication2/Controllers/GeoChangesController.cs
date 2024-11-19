using System;
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
    [Authorize(Roles="User")] // Applies authorization to the entire controller
    public class GeoChangesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GeoChangesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        public async Task<IActionResult> Create(string geoJson, string description, string mapVariant)
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

            var newChange = new GeoChange
            {
                GeoJson = geoJson,
                Description = description,
                UserId = userId
            };

            _context.GeoChanges.Add(newChange);
            await _context.SaveChangesAsync(); // Use await here

            return RedirectToAction("Index", "GeoChanges");
        }

        public async Task<IActionResult> Edit(int? id, string returnUrl)
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

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index");
            return View(geoChange);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,GeoJson,User Id")] GeoChange geoChange, string returnUrl)
        {
            if (id != geoChange.Id)
            {
                return NotFound();
            }

            ModelState.Remove("User ");
            ModelState.Remove("GeoJson");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingGeoChange = await _context.GeoChanges
                        .FirstOrDefaultAsync(g => g.Id == id);

                    if (existingGeoChange == null)
                    {
                        return NotFound();
                    }

                    existingGeoChange.Description = geoChange.Description;

                    _context.Update(existingGeoChange);
                    await _context.SaveChangesAsync(); // Use await here

                    return Redirect(returnUrl ?? Url.Action("Index"));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await GeoChangeExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // You may want to log this exception
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index");
            return View(geoChange);
        }

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
                await _context.SaveChangesAsync(); // Use await here
            }

            // Redirect to the URL provided in returnUrl or default to Index if no return

            // Redirect to the URL provided in returnUrl or default to Index if no returnUrl
            return Redirect(returnUrl ?? Url.Action("Index"));
        }

        // Update the exists check to be async
        private async Task<bool> GeoChangeExists(int id)
        {
            return await _context.GeoChanges.AnyAsync(e => e.Id == id);
        }
    }
}