
select * from TableInfo;
select * from MenuCategories;
select * from MenuItems;
select * from Orders;
select * from Reservations;
select * from OrderItems;

INSERT INTO MenuCategories (Name)
VALUES
(N'Khai vị'),
(N'Đồ uống'),
(N'Món chính');


INSERT INTO MenuItems (Name, Price)
VALUES
-- ===== Khai vị =====
(N'Gỏi cuốn tôm thịt', 35000),
(N'Chả giò rế hải sản', 40000),
(N'Súp bí đỏ kem tươi', 30000),
(N'Salad cá ngừ dầu giấm', 38000),
(N'Bánh mì bơ tỏi', 25000),

-- ===== Đồ uống =====
(N'Cà phê sữa đá', 25000),
(N'Trà đào cam sả', 35000),
(N'Nước cam ép tươi', 30000),
(N'Sinh tố bơ', 40000),
(N'Coca-Cola lon', 20000),

-- ===== Món chính =====
(N'Phở bò tái chín', 50000),
(N'Cơm tấm sườn bì chả', 55000),
(N'Mì xào hải sản', 60000),
(N'Cá hồi áp chảo sốt bơ chanh', 95000),
(N'Gà nướng mật ong', 85000),
(N'Bò lúc lắc khoai tây', 90000),
(N'Cơm chiên Dương Châu', 50000),
(N'Lẩu thái hải sản', 120000),
(N'Bún bò Huế đặc biệt', 55000),
(N'Mì quảng thịt heo', 45000);

-- 5 món khai vị: Id 1–5
UPDATE MenuItems
SET MenuCategoryId = 1   -- Id của 'Khai vị'
WHERE Id BETWEEN 1 AND 5;

-- 5 món đồ uống: Id 6–10
UPDATE MenuItems
SET MenuCategoryId = 2   -- Id của 'Đồ uống'
WHERE Id BETWEEN 6 AND 10;

-- 10 món chính: Id 11–20
UPDATE MenuItems
SET MenuCategoryId = 3   -- Id của 'Món chính'
WHERE Id BETWEEN 11 AND 20;


INSERT INTO TableInfo (Id, IsOccuped, OccupiedById, Since)
VALUES
('A1', 0, NULL, NULL),
('A2', 0, NULL, NULL),
('A3', 0, NULL, NULL),
('A4', 0, NULL, NULL),
('A5', 0, NULL, NULL),
('A6', 0, NULL, NULL),
('A7', 0, NULL, NULL),
('A8', 0, NULL, NULL),
('A9', 0, NULL, NULL),
('A10', 0, NULL, NULL),
('A11', 0, NULL, NULL),
('A12', 0, NULL, NULL),
('A13', 0, NULL, NULL),
('A14', 0, NULL, NULL),
('A15', 0, NULL, NULL),
('A16', 0, NULL, NULL),
('A17', 0, NULL, NULL),
('A18', 0, NULL, NULL),
('A19', 0, NULL, NULL),
('A20', 0, NULL, NULL);


UPDATE TableInfo SET Id = 'A01' WHERE Id = 'A1';
UPDATE TableInfo SET Id = 'A02' WHERE Id = 'A2';
UPDATE TableInfo SET Id = 'A03' WHERE Id = 'A3';
UPDATE TableInfo SET Id = 'A04' WHERE Id = 'A4';
UPDATE TableInfo SET Id = 'A05' WHERE Id = 'A5';
UPDATE TableInfo SET Id = 'A06' WHERE Id = 'A6';
UPDATE TableInfo SET Id = 'A07' WHERE Id = 'A7';
UPDATE TableInfo SET Id = 'A08' WHERE Id = 'A8';
UPDATE TableInfo SET Id = 'A09' WHERE Id = 'A9';

-- Xóa chi tiết món trước (vì phụ thuộc Orders)
DELETE FROM OrderItems;

-- Xóa đơn hàng
DELETE FROM Orders;

-- Xóa đặt bàn
DELETE FROM Reservations;

ALTER TABLE Reservations 
ALTER COLUMN Note nvarchar(max) null;

ALTER TABLE Reservations 
ALTER COLUMN Email nvarchar(max) null;

ALTER TABLE Orders
ADD IsPaid BIT NOT NULL DEFAULT 0,
    PaymentMethod NVARCHAR(50) NULL;