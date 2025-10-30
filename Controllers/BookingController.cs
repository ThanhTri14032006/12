using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantMVC.Models;

namespace RestaurantMVC.Controllers
{
    public class BookingController : Controller
    {
        private readonly RestaurantDbContext _context;

        public BookingController(RestaurantDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Validate booking date and time
                var bookingDateTime = booking.BookingDate.Add(booking.BookingTime);
                
                if (bookingDateTime <= DateTime.Now)
                {
                    ModelState.AddModelError("", "Thời gian đặt bàn phải sau thời điểm hiện tại.");
                    return View("Index", booking);
                }
                
                // Check if restaurant is open (example: 10:00 AM to 10:00 PM)
                var openTime = new TimeSpan(10, 0, 0);
                var closeTime = new TimeSpan(22, 0, 0);
                
                if (booking.BookingTime < openTime || booking.BookingTime > closeTime)
                {
                    ModelState.AddModelError("BookingTime", "Nhà hàng mở cửa từ 10:00 đến 22:00.");
                    return View("Index", booking);
                }
                
                // Check availability (simple check - max 10 bookings per hour)
                var hourStart = booking.BookingDate.Add(new TimeSpan(booking.BookingTime.Hours, 0, 0));
                var hourEnd = hourStart.AddHours(1);
                
                var existingBookings = await _context.Bookings
                    .Where(b => b.BookingDate.Date == booking.BookingDate.Date &&
                               b.BookingTime >= new TimeSpan(booking.BookingTime.Hours, 0, 0) &&
                               b.BookingTime < new TimeSpan(booking.BookingTime.Hours + 1, 0, 0) &&
                               b.Status != BookingStatus.Cancelled)
                    .CountAsync();
                
                if (existingBookings >= 10)
                {
                    ModelState.AddModelError("", "Khung giờ này đã đầy. Vui lòng chọn thời gian khác.");
                    return View("Index", booking);
                }
                
                booking.CreatedAt = DateTime.Now;
                booking.Status = BookingStatus.Pending;
                
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                
                TempData["BookingSuccess"] = $"Đặt bàn thành công! Mã đặt bàn của bạn là: {booking.Id}. Chúng tôi sẽ liên hệ xác nhận sớm nhất.";
                return RedirectToAction("Confirmation", new { id = booking.Id });
            }
            
            return View("Index", booking);
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            
            if (booking == null)
            {
                return NotFound();
            }
            
            return View(booking);
        }

        public async Task<IActionResult> Check(string email, int? bookingId)
        {
            if (string.IsNullOrEmpty(email) && !bookingId.HasValue)
            {
                return View();
            }
            
            var bookings = _context.Bookings.AsQueryable();
            
            if (!string.IsNullOrEmpty(email))
            {
                bookings = bookings.Where(b => b.Email == email);
            }
            
            if (bookingId.HasValue)
            {
                bookings = bookings.Where(b => b.Id == bookingId.Value);
            }
            
            var results = await bookings.OrderByDescending(b => b.CreatedAt).ToListAsync();
            
            return View(results);
        }
    }
}