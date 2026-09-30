PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    Role TEXT NOT NULL CHECK(Role IN ('Admin', 'Personel'))
);

CREATE TABLE IF NOT EXISTS Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT
);

CREATE TABLE IF NOT EXISTS Products (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductCode TEXT NOT NULL UNIQUE,
    ProductName TEXT NOT NULL,
    CategoryId INTEGER NOT NULL,
    BrandModel TEXT,
    CurrentStock INTEGER NOT NULL DEFAULT 0,
    Location TEXT,
    Description TEXT,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS StockMovements (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductId INTEGER NOT NULL,
    MovementType TEXT NOT NULL CHECK(MovementType IN ('Giriş', 'Çıkış')),
    Quantity INTEGER NOT NULL,
    MovementDate TEXT NOT NULL,
    UsagePlace TEXT,
    DeliveredTo TEXT,
    Description TEXT,
    UserId INTEGER,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

INSERT OR IGNORE INTO Users (Username, Password, Role) VALUES
    ('admin', 'admin123', 'Admin'),
    ('personel', 'personel123', 'Personel');

INSERT OR IGNORE INTO Categories (Name, Description) VALUES
    ('Elektronik komponent', 'Direnç, kondansatör, entegre gibi parçalar'),
    ('Kablo', 'Enerji ve veri kabloları'),
    ('Mekanik parça', 'Vida, somun, bağlantı parçaları'),
    ('Sarf malzeme', 'Hızlı tüketilen üretim malzemeleri'),
    ('Test ekipmanı', 'Ölçüm ve kontrol cihazları');
