using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RestaurantMVC.Models
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
        }
        
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure MenuItem
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
            });
            
            // Configure Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SpecialRequests).HasMaxLength(1000);
                entity.Property(e => e.AdminNotes).HasMaxLength(1000);
            });
            
            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FullName).HasMaxLength(200);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });
            
            // Seed data
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@restaurant.com",
                    Password = "admin123", // In production, this should be hashed
                    FullName = "Administrator",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );
            
            // Seed Menu Items
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem { Id = 1, Name = "Phở Bò", Description = "Phở bò truyền thống với nước dùng đậm đà", Price = 65000, Category = "Món chính", ImageUrl = "/images/pho-bo.jpg", IsAvailable = true },
                new MenuItem { Id = 2, Name = "Bún Chả", Description = "Bún chả Hà Nội với thịt nướng thơm ngon", Price = 55000, Category = "Món chính", ImageUrl = "/images/bun-cha.jpg", IsAvailable = true },
                new MenuItem { Id = 3, Name = "Gỏi Cuốn", Description = "Gỏi cuốn tôm thịt tươi ngon", Price = 35000, Category = "Khai vị", ImageUrl = "/images/goi-cuon.jpg", IsAvailable = true },
                new MenuItem { Id = 4, Name = "Chả Cá Lã Vọng", Description = "Chả cá truyền thống với thì là và hành", Price = 85000, Category = "Món chính", ImageUrl = "/images/cha-ca.jpg", IsAvailable = true },
                new MenuItem { Id = 5, Name = "Bánh Mì", Description = "Bánh mì thịt nguội với rau củ tươi", Price = 25000, Category = "Món nhẹ", ImageUrl = "/images/banh-mi.jpg", IsAvailable = true },
                new MenuItem { Id = 6, Name = "Cà Phê Sữa Đá", Description = "Cà phê sữa đá truyền thống", Price = 20000, Category = "Đồ uống", ImageUrl = "/images/ca-phe.jpg", IsAvailable = true }
            );
        }
    }
}