using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantMVC.Models;

namespace RestaurantMVC.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly RestaurantDbContext _context;

        public AdminController(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            var stats = new
            {
                TotalBookingsToday = await _context.Bookings
                    .CountAsync(b => b.BookingDate.Date == today),
                PendingBookings = await _context.Bookings
                    .CountAsync(b => b.Status == BookingStatus.Pending),
                TotalMenuItems = await _context.MenuItems.CountAsync(),
                AvailableMenuItems = await _context.MenuItems
                    .CountAsync(m => m.IsAvailable)
            };
            
            var recentBookings = await _context.Bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();
            
            ViewBag.Stats = stats;
            ViewBag.RecentBookings = recentBookings;
            
            return View();
        }

        // Booking Management
        public async Task<IActionResult> Bookings(string status = "", DateTime? date = null)
        {
            var bookings = _context.Bookings.AsQueryable();
            
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, out var bookingStatus))
            {
                bookings = bookings.Where(b => b.Status == bookingStatus);
            }
            
            if (date.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate.Date == date.Value.Date);
            }
            
            var result = await bookings
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.BookingTime)
                .ToListAsync();
            
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDate = date;
            
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int id, BookingStatus status, string? adminNotes = null)
        {
            var booking = await _context.Bookings.FindAsync(id);
            
            if (booking != null)
            {
                booking.Status = status;
                if (!string.IsNullOrEmpty(adminNotes))
                {
                    booking.AdminNotes = adminNotes;
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật trạng thái đặt bàn thành công.";
            }
            
            return RedirectToAction("Bookings");
        }

        // Menu Management
        public async Task<IActionResult> Menu()
        {
            var menuItems = await _context.MenuItems
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
            
            return View(menuItems);
        }

        public IActionResult CreateMenuItem()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                menuItem.CreatedAt = DateTime.Now;
                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Thêm món ăn thành công.";
                return RedirectToAction("Menu");
            }
            
            return View(menuItem);
        }

        public async Task<IActionResult> EditMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            
            if (menuItem == null)
            {
                return NotFound();
            }
            
            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMenuItem(MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                _context.MenuItems.Update(menuItem);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Cập nhật món ăn thành công.";
                return RedirectToAction("Menu");
            }
            
            return View(menuItem);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleMenuItemAvailability(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            
            if (menuItem != null)
            {
                menuItem.IsAvailable = !menuItem.IsAvailable;
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Đã {(menuItem.IsAvailable ? "kích hoạt" : "vô hiệu hóa")} món ăn.";
            }
            
            return RedirectToAction("Menu");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            
            if (menuItem != null)
            {
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Xóa món ăn thành công.";
            }
            
            return RedirectToAction("Menu");
        }
    }
}