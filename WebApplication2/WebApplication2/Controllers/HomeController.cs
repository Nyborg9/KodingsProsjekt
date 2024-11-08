using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using System.Diagnostics;
using WebApplication2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Add ILogger
        private readonly ApplicationDbContext _context; // Add ApplicationDbContext
        private readonly UserManager<ApplicationUser> _userManager; // Add UserManager

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            // Initialize Logger context and UserManager
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}