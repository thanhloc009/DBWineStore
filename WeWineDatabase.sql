USE master;
GO
IF DB_ID(N'DBWineStore') IS NOT NULL
BEGIN
    ALTER DATABASE DBWineStore SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DBWineStore;
END
GO
CREATE DATABASE DBWineStore;
GO
USE DBWineStore;
GO

/* =========================================================
   1. ROLE / USER ACCOUNT
   ========================================================= */
CREATE TABLE dbo.Role (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE dbo.UserAccount (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PasswordSalt NVARCHAR(100) NOT NULL,
    RoleID INT NOT NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_UserAccount_IsActive DEFAULT(1),
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_UserAccount_CreatedAt DEFAULT(SYSDATETIME()),
    CONSTRAINT FK_UserAccount_Role
        FOREIGN KEY(RoleID) REFERENCES dbo.Role(RoleID)
);

CREATE TABLE dbo.Customer (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL UNIQUE,
    Address NVARCHAR(250) NULL,
    CONSTRAINT FK_Customer_UserAccount
        FOREIGN KEY(UserID) REFERENCES dbo.UserAccount(UserID)
);

/* =========================================================
   2. CATEGORY / PRODUCT
   ImageUrl chỉ lưu TÊN FILE trong Content/images
   ========================================================= */
CREATE TABLE dbo.Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE dbo.Product (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    CategoryID INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(255) NULL,
    StockQuantity INT NOT NULL CONSTRAINT DF_Product_Stock DEFAULT(0),
    IsActive BIT NOT NULL CONSTRAINT DF_Product_IsActive DEFAULT(1),
    CONSTRAINT CK_Product_Price CHECK (Price >= 0),
    CONSTRAINT CK_Product_Stock CHECK (StockQuantity >= 0),
    CONSTRAINT FK_Product_Category
        FOREIGN KEY(CategoryID) REFERENCES dbo.Category(CategoryID)
);

/* =========================================================
   3. ORDER
   ========================================================= */
CREATE TABLE dbo.OrderPro (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    OrderDate DATETIME2 NOT NULL CONSTRAINT DF_OrderPro_Date DEFAULT(SYSDATETIME()),
    CustomerID INT NOT NULL,
    ShippingAddress NVARCHAR(250) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_OrderPro_Total DEFAULT(0),
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_OrderPro_Status DEFAULT(N'Pending'),
    CONSTRAINT FK_OrderPro_Customer
        FOREIGN KEY(CustomerID) REFERENCES dbo.Customer(CustomerID)
);

CREATE TABLE dbo.OrderDetail (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT CK_OrderDetail_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderDetail_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT FK_OrderDetail_OrderPro
        FOREIGN KEY(OrderID) REFERENCES dbo.OrderPro(OrderID),
    CONSTRAINT FK_OrderDetail_Product
        FOREIGN KEY(ProductID) REFERENCES dbo.Product(ProductID)
);
GO

/* =========================================================
   4. SEED ROLE / ACCOUNT
   Password dùng cho LAB:
   Admin      / Admin@123
   Customer01 / Customer@123
   Customer02 / Customer2@123
   ========================================================= */
INSERT INTO dbo.Role(RoleName)
VALUES (N'Admin'), (N'Customer');

INSERT INTO dbo.UserAccount
    (UserName, PasswordHash, PasswordSalt, RoleID, IsActive)
VALUES
    (N'Admin', N'Admin', N'Admin@123',
        (SELECT RoleID FROM dbo.Role WHERE RoleName=N'Admin'), 1),
    (N'Customer01', N'Account1', N'Account1@123',
        (SELECT RoleID FROM dbo.Role WHERE RoleName=N'Customer'), 1),
    (N'Customer02', N'Account2', N'Account2@123',
        (SELECT RoleID FROM dbo.Role WHERE RoleName=N'Customer'), 1);

INSERT INTO dbo.Customer(UserID, FullName, Phone, Email, Address)
VALUES
    ((SELECT UserID FROM dbo.UserAccount WHERE UserName=N'Customer01'),
     N'Nguyễn Minh Anh', N'0335006171', N'minhanh01@gmail.com',
     N'Quận 10, TP.HCM'),
    ((SELECT UserID FROM dbo.UserAccount WHERE UserName=N'Customer02'),
     N'Trần Gia Hân', N'0918234567', N'giahan02@gmail.com',
     N'Quận 3, TP.HCM');

/* =========================================================
   5. SEED CATEGORY  (Wine theme)
   ========================================================= */
INSERT INTO dbo.Category(CategoryName)
VALUES
    (N'Rượu vang đỏ'),
    (N'Rượu vang trắng'),
    (N'Rượu vang hồng & Sủi'),
    (N'Phụ kiện rượu vang');

/* =========================================================
   SEED 48 PRODUCTS (Wine theme) - sorted by CategoryID
   1 = Rượu vang đỏ | 2 = Rượu vang trắng
   3 = Rượu vang hồng & Sủi | 4 = Phụ kiện rượu vang
   Image files: images.zip (file name = ImageUrl)
   ========================================================= */

DECLARE @CatRed INT = (SELECT CategoryID FROM dbo.Category WHERE CategoryName=N'Rượu vang đỏ');
DECLARE @CatWhite INT = (SELECT CategoryID FROM dbo.Category WHERE CategoryName=N'Rượu vang trắng');
DECLARE @CatRoseSparkling INT = (SELECT CategoryID FROM dbo.Category WHERE CategoryName=N'Rượu vang hồng & Sủi');
DECLARE @CatAccessory INT = (SELECT CategoryID FROM dbo.Category WHERE CategoryName=N'Phụ kiện rượu vang');

/* =========================================================
   SEED 48 PRODUCTS (Wine theme) - sorted by CategoryID
   1 = Rượu vang đỏ | 2 = Rượu vang trắng
   3 = Rượu vang hồng & Sủi | 4 = Phụ kiện rượu vang
   Image files: images_renamed.zip (file name = ImageUrl)
   ========================================================= */
-- ---------- 1. Rượu vang đỏ (22 sản phẩm) ----------
INSERT INTO dbo.Product (ProductName, Description, CategoryID, Price, ImageUrl, StockQuantity, IsActive)
VALUES
(N'Cabernet Sauvignon Reserve 2019', N'Vang đỏ đậm đà, hương quả mọng đen và gia vị nhẹ', @CatRed, 890000, N'vangdo-01.jpg', 67, 1),
(N'Merlot Classic 2020', N'Vang đỏ mềm mại, vị trái cây chín, dễ uống', @CatRed, 650000, N'vangdo-02.jpg', 30, 1),
(N'Shiraz Premium 2018', N'Vang đỏ đậm vị, hương tiêu đen và socola', @CatRed, 980000, N'vangdo-03.jpg', 18, 1),
(N'Pinot Noir Estate 2021', N'Vang đỏ thanh lịch, hương anh đào và đất rừng', @CatRed, 1150000, N'vangdo-04.jpg', 15, 1),
(N'Malbec Reserva 2019', N'Vang đỏ Argentina, vị đậm đà và tannin mượt', @CatRed, 790000, N'vangdo-05.jpg', 22, 1),
(N'Oakmont Cabernet 2019', N'Vang đỏ đậm đà, hương mận đen và gỗ sồi, hậu vị dài', @CatRed, 840000, N'vangdo-06.jpg', 34, 1),
(N'Château Brunel Médoc 2018', N'Vang đỏ kiểu Bordeaux, cấu trúc chặt chẽ, hương lý chua đen và khói nhẹ', @CatRed, 1280000, N'vangdo-07.jpg', 14, 1),
(N'Torre Vecchia Montepulciano 2020', N'Vang đỏ Ý, vị trái cây chín, tannin mềm, dễ kết hợp với pasta', @CatRed, 560000, N'vangdo-08.jpg', 38, 1),
(N'Quinta Dorada Douro Red 2019', N'Vang đỏ Bồ Đào Nha, hương quả mọng và gia vị, vị đậm đà', @CatRed, 720000, N'vangdo-09.jpg', 25, 1),
(N'Maple Grove Pinot Noir 2021', N'Vang đỏ nhẹ nhàng, hương anh đào và dâu, tannin mịn', @CatRed, 960000, N'vangdo-10.jpg', 19, 1),
(N'Rio Claro Carmenere 2020', N'Vang đỏ Chile, hương mận, ớt chuông và socola đen', @CatRed, 620000, N'vangdo-11.jpg', 28, 1),
(N'Black Pearl Shiraz 2019', N'Vang đỏ đậm, hương mâm xôi, tiêu đen và khói', @CatRed, 1020000, N'vangdo-12.jpg', 17, 1),
(N'Domaine Belmont Côtes du Rhône 2020', N'Vang đỏ vùng Rhône, hương quả mọng đỏ và thảo mộc khô', @CatRed, 690000, N'vangdo-13.jpg', 24, 1),
(N'Ferrante Barolo 2016', N'Vang đỏ Ý danh tiếng, tannin mạnh, hương hoa hồng, hắc ín và anh đào', @CatRed, 1650000, N'vangdo-14.jpg', 9, 1),
(N'Ironwood Petit Verdot 2019', N'Vang đỏ đậm màu, hương mâm xôi tím và hoa violet, tannin chắc', @CatRed, 930000, N'vangdo-15.jpg', 15, 1),
(N'Baron Lessard Bordeaux Supérieur 2018', N'Vang đỏ Bordeaux, hương lý chua đen, tuyết tùng và vani', @CatRed, 1180000, N'vangdo-16.jpg', 13, 1),
(N'Terra Rossa Aglianico 2018', N'Vang đỏ miền Nam nước Ý, đậm vị, hương mận và khói', @CatRed, 870000, N'vangdo-17.jpg', 16, 1),
(N'Castillo Negro Rioja 2018', N'Vang đỏ Tây Ban Nha ủ gỗ sồi, hương vani, anh đào và da thuộc', @CatRed, 830000, N'vangdo-18.jpg', 23, 1),
(N'Santa Elena Malbec 2020', N'Vang đỏ Argentina, hương mận và hoa tím, tannin mượt', @CatRed, 680000, N'vangdo-19.jpg', 26, 1),
(N'Wren''s Hollow Merlot 2020', N'Vang đỏ mềm mại, hương anh đào đen và mận chín', @CatRed, 600000, N'vangdo-20.jpg', 32, 1),
(N'Domaine Fleur Pinot Noir 2020', N'Vang đỏ Pháp thanh lịch, hương dâu tây và đất ẩm', @CatRed, 1090000, N'vangdo-21.jpg', 12, 1),
(N'Evening Star Red Reserve 2019', N'Vang đỏ pha trộn, hương quả mọng đen, vani, hậu vị mượt', @CatRed, 920000, N'vangdo-22.jpg', 21, 1);

-- ---------- 2. Rượu vang trắng (12 sản phẩm) ----------
INSERT INTO dbo.Product (ProductName, Description, CategoryID, Price, ImageUrl, StockQuantity, IsActive)
VALUES
(N'Chardonnay Barrel 2020', N'Vang trắng ủ thùng gỗ sồi, hương bơ và vani', @CatWhite, 720000, N'vangtrang-01.jpg', 28, 1),
(N'Sauvignon Blanc Fresh 2022', N'Vang trắng tươi mát, hương chanh và thảo mộc', @CatWhite, 580000, N'vangtrang-02.jpg', 35, 1),
(N'Riesling Late Harvest 2021', N'Vang trắng bán ngọt, hương hoa và trái cây chín', @CatWhite, 690000, N'vangtrang-03.jpg', 20, 1),
(N'Pinot Grigio Classic 2022', N'Vang trắng nhẹ nhàng, dễ kết hợp với hải sản', @CatWhite, 550000, N'vangtrang-04.jpg', 32, 1),
(N'Viognier Premium 2020', N'Vang trắng thơm nồng, hương hoa và đào', @CatWhite, 850000, N'vangtrang-05.jpg', 16, 1),
(N'Fieldstone Chardonnay 2021', N'Vang trắng cân bằng, hương táo, lê và chút vani', @CatWhite, 640000, N'vangtrang-06.jpg', 27, 1),
(N'Vallée Claire Blanc 2022', N'Vang trắng thanh nhẹ, hương hoa trắng và trái cây có múi', @CatWhite, 520000, N'vangtrang-07.jpg', 36, 1),
(N'Silver Lake Riesling 2022', N'Vang trắng thanh, độ chua tươi, hương chanh và đào', @CatWhite, 610000, N'vangtrang-08.jpg', 29, 1),
(N'Sunrise Valley Pinot Grigio 2022', N'Vang trắng nhẹ, hương lê và chanh, hợp với salad và hải sản', @CatWhite, 560000, N'vangtrang-09.jpg', 33, 1),
(N'Pebble Creek Sauvignon Blanc 2022', N'Vang trắng tươi mát, hương chanh dây và cỏ tươi', @CatWhite, 590000, N'vangtrang-10.jpg', 31, 1),
(N'Alto Mar Albariño 2022', N'Vang trắng Tây Ban Nha, hương đào, chanh và vị mặn nhẹ như biển', @CatWhite, 740000, N'vangtrang-11.jpg', 18, 1),
(N'Stone Bridge Gewürztraminer 2021', N'Vang trắng thơm nồng, hương vải, hoa hồng và gia vị', @CatWhite, 780000, N'vangtrang-12.jpg', 17, 1);

-- ---------- 3. Rượu vang hồng & Sủi (11 sản phẩm) ----------
INSERT INTO dbo.Product (ProductName, Description, CategoryID, Price, ImageUrl, StockQuantity, IsActive)
VALUES
(N'Rosé Provence 2022', N'Vang hồng Pháp thanh lịch, hương dâu và hoa', @CatRoseSparkling, 680000, N'vanghong-01.jpg', 24, 1),
(N'Sparkling Rosé 2021', N'Vang hồng sủi bọt, hương quả mọng đỏ', @CatRoseSparkling, 750000, N'vanghong-02.jpg', 18, 1),
(N'Castello Rosa Rosé 2022', N'Vang hồng Ý tươi mát, hương dâu và hoa hồng', @CatRoseSparkling, 540000, N'vanghong-03.jpg', 30, 1),
(N'Hawthorn Rosé Reserve 2021', N'Vang hồng đậm đà hơn, hương dâu rừng và cam', @CatRoseSparkling, 710000, N'vanghong-04.jpg', 20, 1),
(N'Prosecco Extra Dry', N'Rượu sủi Ý tươi mát, bọt mịn, dễ uống', @CatRoseSparkling, 490000, N'vangsui-01.jpg', 40, 1),
(N'Champagne Brut Reserve', N'Champagne Pháp cổ điển, sang trọng', @CatRoseSparkling, 1890000, N'vangsui-02.jpg', 12, 1),
(N'Moscato D''Asti', N'Vang ngọt nhẹ, hương hoa và nho chín', @CatRoseSparkling, 520000, N'vangsui-03.jpg', 26, 1),
(N'Les Trois Tours Brut', N'Rượu sủi khô, bọt mịn, hương táo xanh và bánh mì nướng', @CatRoseSparkling, 880000, N'vangsui-04.jpg', 22, 1),
(N'Monte Alto Crémant', N'Rượu sủi Pháp, bọt mịn, hương táo và lê', @CatRoseSparkling, 790000, N'vangsui-05.jpg', 21, 1),
(N'Gold Coast Champagne', N'Champagne Pháp, hương bánh brioche và táo vàng, sang trọng', @CatRoseSparkling, 1750000, N'vangsui-06.jpg', 11, 1),
(N'Mirabelle Brut Rosé', N'Rượu sủi hồng khô, hương dâu và quả mọng đỏ', @CatRoseSparkling, 860000, N'vangsui-07.jpg', 19, 1);

-- ---------- 4. Phụ kiện rượu vang (3 sản phẩm) ----------
INSERT INTO dbo.Product (ProductName, Description, CategoryID, Price, ImageUrl, StockQuantity, IsActive)
VALUES
(N'Bộ mở rượu chuyên nghiệp', N'Gồm đồ khui, nút chai và vòng nhỏ giọt', @CatAccessory, 320000, N'phukien-01.jpg', 45, 1),
(N'Ly vang đỏ (bộ 6)', N'Ly thủy tinh mỏng, thân cao, phù hợp vang đỏ', @CatAccessory, 890000, N'phukien-02.jpg', 20, 1),
(N'Hộp bảo quản rượu (0 chai)', N'Hộp gỗ cao cấp, giữ nhiệt độ ổn định', @CatAccessory, 1250000, N'phukien-03.jpg', 10, 1);

/* =========================================================
   7. SAMPLE ORDER
   ========================================================= */
INSERT INTO dbo.OrderPro
    (OrderDate, CustomerID, ShippingAddress, TotalAmount, Status)
VALUES
    (SYSDATETIME(),
     (SELECT CustomerID FROM dbo.Customer WHERE Email=N'minhanh01@gmail.com'),
     N'Quận 10, TP.HCM',
     2460000,
     N'Completed');

DECLARE @SampleOrderID INT = SCOPE_IDENTITY();

INSERT INTO dbo.OrderDetail(OrderID, ProductID, Quantity, UnitPrice)
VALUES
    (@SampleOrderID,
     (SELECT ProductID FROM dbo.Product WHERE ProductName=N'Cabernet Sauvignon Reserve 2019'),
     1, 890000),
    (@SampleOrderID,
     (SELECT ProductID FROM dbo.Product WHERE ProductName=N'Chardonnay Barrel 2020'),
     1, 720000),
    (@SampleOrderID,
     (SELECT ProductID FROM dbo.Product WHERE ProductName=N'Prosecco Extra Dry'),
     1, 490000),
    (@SampleOrderID,
     (SELECT ProductID FROM dbo.Product WHERE ProductName=N'Bộ mở rượu chuyên nghiệp'),
     1, 360000);   -- adjusted slightly for total

-- Fix total to match exact sum if needed
UPDATE dbo.OrderPro
SET TotalAmount = 890000 + 720000 + 490000 + 320000
WHERE OrderID = @SampleOrderID;
GO

/* =========================================================
   8. QUICK CHECK
   ========================================================= */
SELECT * FROM dbo.Role;
SELECT * FROM dbo.UserAccount;
SELECT * FROM dbo.Customer;
SELECT * FROM dbo.Category;
SELECT * FROM dbo.Product;
SELECT * FROM dbo.OrderPro;
SELECT * FROM dbo.OrderDetail;
GO
