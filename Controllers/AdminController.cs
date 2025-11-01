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
            
            // Get bookings and sort in memory to avoid SQLite TimeSpan ORDER BY issue
            var result = await bookings
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            
            // Sort by BookingTime in memory (now using DateTime.TimeOfDay)
            result = result.OrderByDescending(b => b.BookingDate)
                          .ThenByDescending(b => b.BookingTime.TimeOfDay)
                          .ToList();
            
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

        // Order Management
        public async Task<IActionResult> Orders(string status = "", DateTime? date = null)
        {
            var orders = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem).AsQueryable();
            
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                orders = orders.Where(o => o.Status == orderStatus);
            }
            
            if (date.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt.Date == date.Value.Date);
            }
            
            var result = await orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDate = date;
            
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status, string? notes = null)
        {
            var order = await _context.Orders.FindAsync(id);
            
            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTime.Now;
                if (!string.IsNullOrEmpty(notes))
                {
                    order.Notes = notes;
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công.";
            }
            
            return RedirectToAction("Orders");
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            return View(order);
        }

        // Review Management
        public async Task<IActionResult> Reviews(string status = "")
        {
            var reviews = _context.Reviews.Include(r => r.MenuItem).AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
            {
                bool isApproved = status.ToLower() == "approved";
                reviews = reviews.Where(r => r.IsApproved == isApproved);
            }
            
            var result = await reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            
            ViewBag.SelectedStatus = status;
            
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            
            if (review != null)
            {
                review.IsApproved = true;
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Duyệt đánh giá thành công.";
            }
            
            return RedirectToAction("Reviews");
        }

        [HttpPost]
        public async Task<IActionResult> RejectReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            
            if (review != null)
            {
                review.IsApproved = false;
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Từ chối đánh giá thành công.";
            }
            
            return RedirectToAction("Reviews");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Xóa đánh giá thành công.";
            }
            
            return RedirectToAction("Reviews");
        }

        // User Management
        public async Task<IActionResult> Users(string role = "")
        {
            var users = _context.Users.AsQueryable();
            
            if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, out var userRole))
            {
                users = users.Where(u => u.Role == userRole);
            }
            
            var result = await users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            
            ViewBag.SelectedRole = role;
            
            return View(result);
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if username or email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == user.Username || u.Email == user.Email);
                
                if (existingUser != null)
                {
                    if (existingUser.Username == user.Username)
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    if (existingUser.Email == user.Email)
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                    
                    return View(user);
                }
                
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Tạo người dùng thành công.";
                return RedirectToAction("Users");
            }
            
            return View(user);
        }

        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if username or email already exists (excluding current user)
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id != user.Id && (u.Username == user.Username || u.Email == user.Email));
                
                if (existingUser != null)
                {
                    if (existingUser.Username == user.Username)
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    if (existingUser.Email == user.Email)
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                    
                    return View(user);
                }
                
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Cập nhật người dùng thành công.";
                return RedirectToAction("Users");
            }
            
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Đã {(user.IsActive ? "kích hoạt" : "vô hiệu hóa")} người dùng.";
            }
            
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Xóa người dùng thành công.";
            }
            
            return RedirectToAction("Users");
        }
    }
}