# Hướng dẫn chuyển đổi Database

## Hiện tại đang sử dụng: SQLite

### Để chuyển sang SQL Server:

1. **Cập nhật appsettings.json:**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=RestaurantDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;Encrypt=false;"
   }
   ```

2. **Cập nhật Program.cs:**
   ```csharp
   builder.Services.AddDbContext<RestaurantDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```

3. **Chạy migration:**
   ```bash
   dotnet ef database update
   ```

### Để chuyển về SQLite:

1. **Cập nhật appsettings.json:**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=restaurant.db"
   }
   ```

2. **Cập nhật Program.cs:**
   ```csharp
   builder.Services.AddDbContext<RestaurantDbContext>(options =>
       options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```

## Lưu ý về SQL Server trên macOS:

- Cần cài đặt SQL Server for macOS hoặc sử dụng Docker
- Đảm bảo SQL Server đang chạy trên port 1433
- Cập nhật password trong connection string cho phù hợp
- Có thể cần cấu hình firewall để cho phép kết nối

## Lỗi thường gặp:

1. **Kerberos Authentication Error**: Sử dụng SQL Authentication thay vì Windows Authentication
2. **Database không tồn tại**: Chạy `dotnet ef database update` để tạo database
3. **Login failed**: Kiểm tra username/password trong connection string

## Kiểm tra kết nối SQL Server:

```bash
# Kiểm tra SQL Server có đang chạy không
docker ps | grep sql

# Hoặc kiểm tra port
netstat -an | grep 1433
```