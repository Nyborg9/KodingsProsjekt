using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin, Caseworker")]
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

        // Overview of all reports
        [Authorize(Roles = "Admin, Caseworker")]
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

        // Details of a specific report
        [HttpGet]
        public IActionResult ReportDetails(int id)
        {
            var report = _context.GeoChanges.Find(id); // Fetch the specific report by ID
            if (report == null)
            {
                return NotFound();
            }
            return View(report);
        }

        // Edit a specific report
        [HttpGet]
        public async Task<IActionResult> EditReport(int? id)
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
            return View(geoChange);
        }

        // Updates the description of a existing report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReport(int id, [Bind("Description")] GeoChange geoChange)
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

                // Redirect to the CaseworkerOverview after successful edit
                return RedirectToAction("CaseworkerOverview", "Caseworker");
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error updating GeoChange: {ex.Message}");

                // Return the view with the current model if an error occurs
                return View(geoChange);
            }
        }

        // Shows the user deletion confirmation page
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
        // Deletes the user after confirmation
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
                    // sign out the user after deletion
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


        // Shows the report deletion confirmation page
        [HttpGet]
        public async Task<IActionResult> DeleteReport(int? id)
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

        // POST: Caseworker/Delete/5
        // Deletes the report after confirmation
        [HttpPost, ActionName("DeleteReport")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Fetch the geoChange entity
            var geoChange = await _context.GeoChanges.FindAsync(id);

            if (geoChange == null)
            {
                return NotFound();  // Return 404 if entity not found
            }

            // Remove the entity from the database
            _context.GeoChanges.Remove(geoChange);
            await _context.SaveChangesAsync();

            // Redirect to CaseworkerOverview after deletion
            return RedirectToAction("CaseworkerOverview", "Caseworker");
        }

        // Update the status and priority of a specific report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatusAndPriority(int id, ReportStatus status, PriorityLevel priority)
        {
            var geoChange = await _context.GeoChanges.FindAsync(id);
            if (geoChange == null)
            {
                return NotFound();
            }

            // Update the properties
            geoChange.Status = status;
            geoChange.Priority = priority;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect back to the ReportDetails page (the same page) with the updated data
            return RedirectToAction("ReportDetails", new { id = geoChange.Id });
        }

        // Details of a specific report
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var geoChange = await _context.GeoChanges.FindAsync(id);
            if (geoChange == null)
            {
                return NotFound();
            }

            return View(geoChange);
        }

        // List all users, excluding admin
        [HttpGet]
        public async Task<IActionResult> UserList()
        {
            // Get the currently logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            bool isAdmin = currentUserRoles.Contains("Admin");

            // Get all users, excluding the admin user
            var users = await _userManager.Users
                .Where(user => user.Email != "admin@admin.com")
                .ToListAsync();

            var userViewModels = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Only add normal users if the current user is a caseworker
                if (!isAdmin && roles.Contains("Caseworker"))
                {
                    continue; // Skip caseworkers if the current user is not an admin
                }

                userViewModels.Add(new UserListViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsUser = roles.Contains("User"),
                    IsCaseworker = roles.Contains("Caseworker")
                });
            }

            // Pass the isAdmin flag to the view
            ViewBag.IsAdmin = isAdmin;

            return View(userViewModels);
        }

        // Promotes a user to caseworker, only accessible by admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToCaseworker(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Caseworker");
            }
            return RedirectToAction("UserList");
        }

        // Demote a caseworker to regular user, only accessible by admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoteToUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Caseworker");
                await _userManager.AddToRoleAsync(user, "User");
            }
            return RedirectToAction("UserList");
        }
    }
}