using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;

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

        public IActionResult ReportDetails(int id)
        {
            var report = _context.GeoChanges.Find(id); // Fetch the specific report by ID
            if (report == null)
            {
                return NotFound();
            }
            return View(report);
        }
    }
}
