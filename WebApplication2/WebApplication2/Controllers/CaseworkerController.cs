using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CaseworkerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CaseworkerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Initializes UserManager and Context
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CaseworkerOverview()
        {
            var changes_db = _context.GeoChanges.ToList();
            if (changes_db == null || !changes_db.Any())
            {
                return View(new List<GeoChange>()); // Specify the view name
            }
            return View(changes_db); // Specify the view name
        }

        public async Task<IActionResult> UserList()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = users.Select(user => new UserListViewModel
            {
                Id = user.Id,
                Email = user.Email
            }).ToList();

            return View(userViewModels);
        }


        public IActionResult ReportDetails(int id)
        {
            var report = _context.GeoChanges.Find(id); // Fetch the specific report by ID
            if (report == null)
            {
                return NotFound();
            }
            return View(report);
        }
        public async Task<IActionResult> EditReport(int? id, string returnUrl)
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
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("CaseworkerOverview"); //default to index
            return View(geoChange);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReport(int id, [Bind("Id,Description,GeoJson,UserId")] GeoChange geoChange, string returnUrl)
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
                    return Redirect(returnUrl ?? Url.Action("CaseworkerOverview"));

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
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("CaseworkerOverview");
            return View(geoChange);
        }   


        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new DeleteUserViewModel
            {
                Id = user.Id,
                Email = user.Email
            };

            return View(viewModel); // Pass ViewModel to the view
        }

        // POST: Caseworker/DeleteUser/{id}
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    // Optionally, sign out the user after deletion
                    await HttpContext.SignOutAsync();
                    return RedirectToAction("Userlist");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(new DeleteUserViewModel { Id = id, Email = user?.Email });
        }
        public async Task<IActionResult> DeleteReport(int? id, string returnUrl)
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


        // POST: Caseworker/Delete/5
        [HttpPost, ActionName("DeleteReport")]
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
            return Redirect(returnUrl ?? Url.Action("CaseworkerOverview"));
        }
    }
}
