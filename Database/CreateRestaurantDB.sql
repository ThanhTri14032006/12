-- Tạo database RestaurantDB cho SQL Server
-- Script này sẽ tạo database và tất cả các bảng cần thiết cho hệ thống quản lý nhà hàng

USE RestaurantDB;
GO

-- Xóa database nếu đã tồn tại
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'RestaurantDB')
BEGIN
    ALTER DATABASE RestaurantDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RestaurantDB;
END
GO

-- Tạo database mới
CREATE DATABASE RestaurantDB;
GO

USE RestaurantDB;
GO

-- Tạo bảng Users (Quản lý người dùng)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Customer',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Tạo bảng MenuItems (Quản lý sản phẩm/món ăn)
CREATE TABLE MenuItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Price DECIMAL(10,2) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    ImageUrl NVARCHAR(255),
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Tạo bảng Bookings (Quản lý đơn đặt bàn)
CREATE TABLE Bookings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    BookingDate DATE NOT NULL,
    BookingTime TIME NOT NULL,
    PartySize INT NOT NULL,
    SpecialRequests NVARCHAR(500),
    Status INT NOT NULL DEFAULT 0, -- 0: Pending, 1: Confirmed, 2: Cancelled, 3: Completed
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    AdminNotes NVARCHAR(500)
);

-- Tạo bảng Reviews (Quản lý đánh giá)
CREATE TABLE Reviews (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(1000),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsApproved BIT NOT NULL DEFAULT 0
);

-- Tạo bảng Orders (Quản lý đơn hàng)
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    DeliveryAddress NVARCHAR(500),
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0: Pending, 1: Confirmed, 2: Preparing, 3: Ready, 4: Delivered, 5: Cancelled
    OrderType INT NOT NULL DEFAULT 0, -- 0: Delivery, 1: Pickup
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(500)
);

-- Tạo bảng OrderItems (Chi tiết đơn hàng)
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    MenuItemId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (MenuItemId) REFERENCES MenuItems(Id)
);

-- Tạo bảng __EFMigrationsHistory cho Entity Framework
CREATE TABLE __EFMigrationsHistory (
    MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
    ProductVersion NVARCHAR(32) NOT NULL
);

-- Thêm dữ liệu mẫu cho Users
INSERT INTO Users (Username, Email, PasswordHash, Role) VALUES
('admin', 'admin@restaurant.com', 'AQAAAAEAACcQAAAAEJ5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q==', 'Admin'),
('manager', 'manager@restaurant.com', 'AQAAAAEAACcQAAAAEJ5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q==', 'Manager');

-- Thêm dữ liệu mẫu cho MenuItems
INSERT INTO MenuItems (Name, Description, Price, Category, ImageUrl, IsAvailable) VALUES
('Phở Bò Tái', 'Phở bò tái truyền thống với nước dùng đậm đà', 65000, 'Món chính', '/images/pho-bo-tai.jpg', 1),
('Bún Chả Hà Nội', 'Bún chả Hà Nội với thịt nướng thơm ngon', 70000, 'Món chính', '/images/bun-cha.jpg', 1),
('Cơm Tấm Sườn Nướng', 'Cơm tấm sườn nướng với chả trứng', 75000, 'Món chính', '/images/com-tam.jpg', 1),
('Bánh Mì Thịt Nướng', 'Bánh mì thịt nướng với rau sống', 25000, 'Món nhẹ', '/images/banh-mi.jpg', 1),
('Chè Ba Màu', 'Chè ba màu truyền thống', 20000, 'Tráng miệng', '/images/che-ba-mau.jpg', 1),
('Cà Phê Đen', 'Cà phê đen truyền thống', 15000, 'Đồ uống', '/images/ca-phe-den.jpg', 1),
('Trà Đá', 'Trà đá mát lạnh', 5000, 'Đồ uống', '/images/tra-da.jpg', 1),
('Nem Rán', 'Nem rán giòn với nước chấm', 40000, 'Khai vị', '/images/nem-ran.jpg', 1);

-- Thêm dữ liệu mẫu cho Bookings
INSERT INTO Bookings (CustomerName, Email, Phone, BookingDate, BookingTime, PartySize, SpecialRequests, Status) VALUES
('Nguyễn Văn A', 'nguyenvana@email.com', '0901234567', '2024-12-01', '19:00:00', 4, 'Bàn gần cửa sổ', 1),
('Trần Thị B', 'tranthib@email.com', '0912345678', '2024-12-02', '18:30:00', 2, '', 0),
('Lê Văn C', 'levanc@email.com', '0923456789', '2024-12-03', '20:00:00', 6, 'Sinh nhật', 1);

-- Thêm dữ liệu mẫu cho Reviews
INSERT INTO Reviews (CustomerName, Email, Rating, Comment, IsApproved) VALUES
('Nguyễn Văn D', 'nguyenvand@email.com', 5, 'Món ăn rất ngon, phục vụ tốt!', 1),
('Trần Thị E', 'tranthie@email.com', 4, 'Không gian đẹp, giá cả hợp lý', 1),
('Lê Văn F', 'levanf@email.com', 5, 'Sẽ quay lại lần sau', 0);

-- Thêm dữ liệu mẫu cho Orders
INSERT INTO Orders (CustomerName, Email, Phone, DeliveryAddress, TotalAmount, Status, OrderType, Notes) VALUES
('Phạm Văn G', 'phamvang@email.com', '0934567890', '123 Đường ABC, Quận 1, TP.HCM', 140000, 1, 0, 'Giao hàng trước 12h'),
('Hoàng Thị H', 'hoangthih@email.com', '0945678901', '', 95000, 2, 1, 'Đến lấy lúc 13h30');

-- Thêm dữ liệu mẫu cho OrderItems
INSERT INTO OrderItems (OrderId, MenuItemId, Quantity, UnitPrice, TotalPrice) VALUES
(1, 1, 2, 65000, 130000),
(1, 6, 1, 15000, 15000),
(2, 2, 1, 70000, 70000),
(2, 4, 1, 25000, 25000);

-- Tạo indexes để tối ưu hiệu suất
CREATE INDEX IX_Bookings_BookingDate ON Bookings(BookingDate);
CREATE INDEX IX_Bookings_Status ON Bookings(Status);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);
CREATE INDEX IX_MenuItems_Category ON MenuItems(Category);
CREATE INDEX IX_MenuItems_IsAvailable ON MenuItems(IsAvailable);

PRINT 'Database RestaurantDB đã được tạo thành công với tất cả bảng và dữ liệu mẫu!';