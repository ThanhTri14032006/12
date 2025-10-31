using Microsoft.AspNetCore.Mvc;
using RestaurantMVC.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace RestaurantMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RestaurantDbContext _context;

        public HomeController(ILogger<HomeController> logger, RestaurantDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured menu items for homepage
            var featuredItems = await _context.MenuItems
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.Id)
                .Take(6)
                .ToListAsync();
            
            ViewBag.FeaturedItems = featuredItems;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would send an email or save to database
                TempData["ContactSuccess"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
                return RedirectToAction("Contact");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
