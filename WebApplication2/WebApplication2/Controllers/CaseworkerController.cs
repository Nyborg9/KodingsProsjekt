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

        // POST: User/Delete/{id}
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
    }
}
