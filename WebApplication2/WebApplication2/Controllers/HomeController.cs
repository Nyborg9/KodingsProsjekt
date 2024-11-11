using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Add ILogger
        private readonly ApplicationDbContext _context; // Add ApplicationDbContext
        private readonly UserManager<ApplicationUser> _userManager; // Add UserManager

        // Initialize Logger, context and UserManager
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}