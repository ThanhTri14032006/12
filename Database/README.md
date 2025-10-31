# Hướng dẫn thiết lập SQL Server Database

## Yêu cầu
- SQL Server (LocalDB, Express, hoặc Full version)
- SQL Server Management Studio (SSMS) hoặc Azure Data Studio

## Cách chạy SQL Script

### Phương pháp 1: Sử dụng SQL Server Management Studio (SSMS)
1. Mở SQL Server Management Studio
2. Kết nối đến SQL Server instance của bạn
3. Mở file `CreateRestaurantDB.sql`
4. Nhấn F5 hoặc click "Execute" để chạy script

### Phương pháp 2: Sử dụng Command Line (sqlcmd)
```bash
sqlcmd -S localhost -U sa -P ThanhTri1403@ -i CreateRestaurantDB.sql
```

### Phương pháp 3: Sử dụng Azure Data Studio
1. Mở Azure Data Studio
2. Kết nối đến SQL Server
3. Mở file `CreateRestaurantDB.sql`
4. Nhấn F5 để chạy script

## Cấu hình Connection String

Connection string trong `appsettings.json` đã được cấu hình:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RestaurantDB;User Id=sa;Password=ThanhTri1403@;TrustServerCertificate=True;"
  }
}
```

**Lưu ý:** Thay đổi thông tin kết nối phù hợp với SQL Server của bạn:
- `Server`: Địa chỉ SQL Server (localhost, IP address, hoặc server name)
- `User Id`: Username SQL Server
- `Password`: Password SQL Server

## Cấu trúc Database

Database `RestaurantDB` bao gồm các bảng:

### 1. **Users** - Quản lý người dùng
- Id, Username, Email, PasswordHash, Role, CreatedAt, IsActive

### 2. **MenuItems** - Quản lý sản phẩm/món ăn
- Id, Name, Description, Price, Category, ImageUrl, IsAvailable, CreatedAt, UpdatedAt

### 3. **Bookings** - Quản lý đặt bàn
- Id, CustomerName, Email, Phone, BookingDate, BookingTime, PartySize, SpecialRequests, Status, CreatedAt, AdminNotes

### 4. **Orders** - Quản lý đơn hàng
- Id, CustomerName, Email, Phone, DeliveryAddress, TotalAmount, Status, OrderType, CreatedAt, UpdatedAt, Notes

### 5. **OrderItems** - Chi tiết đơn hàng
- Id, OrderId, MenuItemId, Quantity, UnitPrice, TotalPrice

### 6. **Reviews** - Quản lý đánh giá
- Id, CustomerName, Email, Rating, Comment, CreatedAt, IsApproved

## Dữ liệu mẫu

Script đã bao gồm dữ liệu mẫu cho:
- 2 tài khoản admin/manager
- 8 món ăn mẫu
- 3 booking mẫu
- 3 review mẫu
- 2 đơn hàng mẫu với chi tiết

## Chạy ứng dụng

Sau khi chạy SQL script thành công:

1. Mở terminal trong thư mục project
2. Chạy lệnh: `dotnet run`
3. Truy cập: `http://localhost:5253`

## Tài khoản đăng nhập mẫu

- **Admin**: admin@restaurant.com / password123
- **Manager**: manager@restaurant.com / password123

*Lưu ý: Mật khẩu đã được hash, bạn cần cập nhật lại mật khẩu thực tế trong code.*