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
        public async Task<IActionResult> Create(string bookingTime, string customerName, string email, string phone, DateTime bookingDate, int partySize, string specialRequests = "")
        {
            try
            {
                Console.WriteLine($"Received booking request - Time: {bookingTime}, Customer: {customerName}, Email: {email}, Phone: {phone}, Date: {bookingDate}, Party Size: {partySize}");
                
                // Parse the time string to DateTime (time only)
                if (!DateTime.TryParse(bookingTime, out DateTime parsedTime))
                {
                    Console.WriteLine($"Failed to parse booking time: {bookingTime}");
                    ModelState.AddModelError("BookingTime", "Giờ đặt bàn không hợp lệ.");
                    var errorBooking = new Booking 
                    { 
                        CustomerName = customerName, 
                        Email = email, 
                        Phone = phone, 
                        BookingDate = bookingDate, 
                        PartySize = partySize, 
                        SpecialRequests = specialRequests ?? "" 
                    };
                    return View("Index", errorBooking);
                }
                
                var booking = new Booking
                {
                    CustomerName = customerName,
                    Email = email,
                    Phone = phone,
                    BookingDate = bookingDate,
                    BookingTime = parsedTime, // Now using DateTime
                    PartySize = partySize,
                    SpecialRequests = specialRequests ?? ""
                };
                
                Console.WriteLine($"Created booking object - Time: {booking.BookingTime:HH:mm}");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is invalid:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    return View("Index", booking);
                }
                else
                {
                    Console.WriteLine("ModelState is valid, proceeding with booking...");
                    
                    // Check if restaurant is open (example: 10:00 AM to 10:00 PM)
                    var openTime = new TimeSpan(10, 0, 0);
                    var closeTime = new TimeSpan(22, 0, 0);
                    var bookingTimeSpan = booking.BookingTime.TimeOfDay;
                    Console.WriteLine($"Restaurant hours: {openTime} - {closeTime}, Booking time: {bookingTimeSpan}");
                    
                    if (bookingTimeSpan < openTime || bookingTimeSpan > closeTime)
                    {
                        Console.WriteLine("Booking time is outside restaurant hours");
                        ModelState.AddModelError("BookingTime", "Nhà hàng mở cửa từ 10:00 đến 22:00.");
                        return View("Index", booking);
                    }
                    
                    Console.WriteLine("Checking availability...");
                    // Simplified availability check to avoid LINQ translation issues
                    var existingBookingsCount = await _context.Bookings
                        .Where(b => b.BookingDate.Date == booking.BookingDate.Date &&
                                   b.BookingTime.TimeOfDay == booking.BookingTime.TimeOfDay &&
                                   (int)b.Status != 2) // Not cancelled
                        .CountAsync();
                    
                    Console.WriteLine($"Existing bookings at this time: {existingBookingsCount}");
                    
                    if (existingBookingsCount >= 5) // Reduced limit for testing
                    {
                        Console.WriteLine("Time slot is full");
                        ModelState.AddModelError("", "Khung giờ này đã đầy. Vui lòng chọn thời gian khác.");
                        return View("Index", booking);
                    }
                    
                    booking.CreatedAt = DateTime.Now;
                    booking.Status = BookingStatus.Pending;
                    Console.WriteLine($"Setting booking status to: {booking.Status}, CreatedAt: {booking.CreatedAt}");
                    
                    Console.WriteLine("Adding booking to database...");
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"Booking saved successfully with ID: {booking.Id}");
                    
                    TempData["BookingSuccess"] = $"Đặt bàn thành công! Mã đặt bàn của bạn là: {booking.Id}. Chúng tôi sẽ liên hệ xác nhận sớm nhất.";
                    Console.WriteLine($"Redirecting to Confirmation with ID: {booking.Id}");
                    return RedirectToAction("Confirmation", new { id = booking.Id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt bàn. Vui lòng thử lại.");
                
                var errorBooking = new Booking 
                { 
                    CustomerName = customerName, 
                    Email = email, 
                    Phone = phone, 
                    BookingDate = bookingDate, 
                    PartySize = partySize, 
                    SpecialRequests = specialRequests ?? "" 
                };
                return View("Index", errorBooking);
            }
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

        public IActionResult Check()
        {
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }

        [HttpPost]
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
            
            ViewBag.Bookings = results;
            return View();
        }
    }
}