using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

public class GeoChangeController : Controller
{
    private readonly ApplicationDbContext _context;

    public GeoChangeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // List all reports
    public IActionResult Index()
    {
        var reports = _context.GeoChanges
            .Include(g => g.Caseworker) // Include Caseworker data
            .ToList();
        return View(reports);
    }

    // View details of a report
    public IActionResult Details(int id)
    {
        var report = _context.GeoChanges
            .Include(g => g.Caseworker)
            .FirstOrDefault(g => g.Id == id);

        if (report == null)
        {
            return NotFound();
        }

        return View(report);
    }

    // Update report status
    [HttpPost]
    public IActionResult UpdateStatus(int id, string newStatus)
    {
        var report = _context.GeoChanges.Find(id);
        if (report != null)
        {
            report.Status = newStatus; // Update status
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}