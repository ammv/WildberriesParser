USE master;

DROP DATABASE IF EXISTS WBParser;
go
Create database WBParser;
go

use WBParser;

Create Table [Role]
(
	ID INT PRIMARY KEY IDENTITY(1,1),
	Name nvarchar(50) NOT NULL
)

print '������� Role �������!'

INSERT INTO [Role] VALUES
('admin'),
('staff')

print '������� ���� "admin" � "staff" ���������!'

Create Table [LogType]
(
	ID INT PRIMARY KEY IDENTITY(1,1),
	Name nvarchar(50) NOT NULL
)

print '������� LogType �������!'

INSERT INTO LogType VALUES
('COMMON'),
('AUTH_USER'),
('EXIT_USER'),
('CREATE_USER'),
('AUTH_USER_TRY'),
('FIND_PRODUCTS'),
('CHANGE_DB_SETTINGS'),
('DELETE_USER'),
('LOAD_UPDATE'),
('ERROR')

print '������� ���� ����� ���������!'


Create Table [User]
(
	ID INT PRIMARY KEY IDENTITY(1,1),
	[Login] NVARCHAR(50) NOT NULL,
	[Password] NVARCHAR(50) NOT NULL,
	RoleID INT FOREIGN KEY REFERENCES Role(ID),
    CanAuth bit default 1 NOT NULL
)

print '������� User �������!'

INSERT INTO [User] VALUES
('admin', '12345', 1, DEFAULT),
('staff', '12345', 2, DEFAULT)

print '������� ������������ "admin:12345" � "staff:12345" ���������!'

Create Table [Log]
(
	ID INT PRIMARY KEY IDENTITY(1,1),
	[UserID] INT FOREIGN KEY REFERENCES [User](ID),
	[Type] int FOREIGN KEY REFERENCES LogType(ID),
	[Description] nvarchar(300) null,
	[Date] datetime not null
)

print '������� Logs �������!'

Create Table [WbBrand]
(
	ID INT PRIMARY KEY,
    [Name] nvarchar(100) NOT NULL
)

print '������� WbBrand �������!'

Create Table [WbProduct]
(
	ID INT PRIMARY KEY,
    [Name] nvarchar(200) NOT NULL,
    WbBrandID INT FOREIGN KEY REFERENCES WbBrand(ID),
    LastDiscount int,
    LastPriceWithDiscount decimal(10,2),
    LastPriceWithoutDiscount decimal(10,2),
    LastUpdate date NOT NULL
)

print '������� WbProduct �������!'

Create Table [WbProductChanges]
(
	ID INT PRIMARY KEY IDENTITY(1,1),
	WbProductID INT FOREIGN KEY REFERENCES WbProduct(ID) ,
    [Date] date NOT NULL,
    Discount int NOT NULL,
    PriceWithDiscount decimal(10,2) NOT NULL,
    PriceWithoutDiscount decimal(10,2) NOT NULL,
    Quantity int NOT NULL
)

print '������� WbProductChanges �������!'

Create Table [SearchPatternType]
(
	ID INT PRIMARY KEY identity(1,1),
    [Name] nvarchar(30) NOT NULL,
)

print '������� SearchPatternType �������!'

INSERT INTO [SearchPatternType] VALUES
('��� ������'),
('ID ������'),
('������� ������')

print '������� ���� ��������� ��������� ���������!'

Create Table [WbProductPosChanges]
(
	ID INT PRIMARY KEY IDENTITY(1,1),
	WbProductID INT FOREIGN KEY REFERENCES WbProduct(ID),
    [Date] date NOT NULL,
    [SearchPatternTypeID] INT FOREIGN KEY REFERENCES SearchPatternType(ID),
    SearchPattern nvarchar(100) NOT NULL,
    Position int  NOT NULL,
    [Page] int NOT NULL
)

print ''
print '============================'
print '���� ������ ������� �������'
print '============================'