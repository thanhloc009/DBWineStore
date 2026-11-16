
USE DBWineStore;
GO

-- 1. Payment Method
CREATE TABLE dbo.PaymentMethod (
    PaymentMethodID INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(30) NOT NULL UNIQUE,
    MethodName NVARCHAR(100) NOT NULL
);

-- 2. Payment Status
CREATE TABLE dbo.PaymentStatus (
    PaymentStatusID INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(30) NOT NULL UNIQUE,
    StatusName NVARCHAR(100) NOT NULL
);

-- 3. Shipping Status
CREATE TABLE dbo.ShippingStatus (
    ShippingStatusID INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(30) NOT NULL UNIQUE,
    StatusName NVARCHAR(100) NOT NULL
);
GO

-- Insert Payment Methods
INSERT INTO dbo.PaymentMethod (Code, MethodName)
VALUES
(N'COD', N'Thanh toán khi nhận hàng'),
(N'BANK', N'Chuyển khoản ngân hàng');

-- Insert Payment Status
INSERT INTO dbo.PaymentStatus (Code, StatusName)
VALUES
(N'PENDING', N'Chưa thanh toán'),
(N'PAID', N'Đã thanh toán'),
(N'FAILED', N'Thanh toán thất bại'),
(N'REFUNDED', N'Đã hoàn tiền');

-- Insert Shipping Status
INSERT INTO dbo.ShippingStatus (Code, StatusName)
VALUES
(N'PENDING', N'Chờ xác nhận'),
(N'PREPARING', N'Đang chuẩn bị hàng'),
(N'SHIPPING', N'Đang vận chuyển'),
(N'DELIVERED', N'Giao hàng thành công'),
(N'FAILED', N'Giao hàng thất bại'),
(N'RETURNED', N'Đã hoàn hàng');
GO

-- Add columns to OrderPro
ALTER TABLE dbo.OrderPro ADD
    PaymentMethodID INT NULL,
    PaymentStatusID INT NULL,
    ShippingStatusID INT NULL;
GO

-- Set default values for existing orders
UPDATE dbo.OrderPro
SET
    PaymentMethodID = 1,
    PaymentStatusID = 1,
    ShippingStatusID = 1;
GO

-- Add foreign keys
ALTER TABLE dbo.OrderPro
ADD CONSTRAINT FK_OrderPro_PaymentMethod
FOREIGN KEY (PaymentMethodID)
REFERENCES dbo.PaymentMethod(PaymentMethodID);

ALTER TABLE dbo.OrderPro
ADD CONSTRAINT FK_OrderPro_PaymentStatus
FOREIGN KEY (PaymentStatusID)
REFERENCES dbo.PaymentStatus(PaymentStatusID);

ALTER TABLE dbo.OrderPro
ADD CONSTRAINT FK_OrderPro_ShippingStatus
FOREIGN KEY (ShippingStatusID)
REFERENCES dbo.ShippingStatus(ShippingStatusID);
GO

-- Check result
SELECT * FROM dbo.PaymentMethod;
SELECT * FROM dbo.PaymentStatus;
SELECT * FROM dbo.ShippingStatus;

SELECT * FROM dbo.OrderPro;
GO
