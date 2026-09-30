using System.Data;
using Microsoft.Data.Sqlite;

namespace StokTakipSistemi;

public static class Database
{
    private static readonly string DatabasePath = Path.Combine(AppContext.BaseDirectory, "stok_takip.db");
    private static readonly string ConnectionString = $"Data Source={DatabasePath}";

    public static void Initialize()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
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
            """;
        command.ExecuteNonQuery();
    }

    public static SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        return connection;
    }

    public static UserSession? Authenticate(string username, string password)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Username, Role
            FROM Users
            WHERE Username = $username AND Password = $password
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$username", username.Trim());
        command.Parameters.AddWithValue("$password", password.Trim());

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new UserSession
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Role = reader.GetString(2)
        };
    }

    public static DataTable Query(string sql, params SqliteParameter[] parameters)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);

        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public static int Execute(string sql, params SqliteParameter[] parameters)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        command.Parameters.AddRange(parameters);
        var affectedRows = command.ExecuteNonQuery();
        transaction.Commit();
        return affectedRows;
    }

    public static int ExecuteStockMovement(
        int productId,
        string movementType,
        int quantity,
        DateTime movementDate,
        string usagePlace,
        string deliveredTo,
        string description,
        int userId)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        using (var checkCommand = connection.CreateCommand())
        {
            checkCommand.Transaction = transaction;
            checkCommand.CommandText = "SELECT CurrentStock FROM Products WHERE Id = $id;";
            checkCommand.Parameters.AddWithValue("$id", productId);

            var currentStock = Convert.ToInt32(checkCommand.ExecuteScalar());
            if (movementType == "Çıkış" && quantity > currentStock)
            {
                throw new InvalidOperationException("Çıkış miktarı mevcut stoktan fazla olamaz.");
            }
        }

        using (var updateCommand = connection.CreateCommand())
        {
            updateCommand.Transaction = transaction;
            updateCommand.CommandText = movementType == "Giriş"
                ? "UPDATE Products SET CurrentStock = CurrentStock + $quantity WHERE Id = $id;"
                : "UPDATE Products SET CurrentStock = CurrentStock - $quantity WHERE Id = $id;";
            updateCommand.Parameters.AddWithValue("$quantity", quantity);
            updateCommand.Parameters.AddWithValue("$id", productId);
            updateCommand.ExecuteNonQuery();
        }

        using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO StockMovements
                    (ProductId, MovementType, Quantity, MovementDate, UsagePlace, DeliveredTo, Description, UserId)
                VALUES
                    ($productId, $movementType, $quantity, $movementDate, $usagePlace, $deliveredTo, $description, $userId);
                """;
            insertCommand.Parameters.AddWithValue("$productId", productId);
            insertCommand.Parameters.AddWithValue("$movementType", movementType);
            insertCommand.Parameters.AddWithValue("$quantity", quantity);
            insertCommand.Parameters.AddWithValue("$movementDate", movementDate.ToString("yyyy-MM-dd"));
            insertCommand.Parameters.AddWithValue("$usagePlace", usagePlace);
            insertCommand.Parameters.AddWithValue("$deliveredTo", deliveredTo);
            insertCommand.Parameters.AddWithValue("$description", description);
            insertCommand.Parameters.AddWithValue("$userId", userId);
            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
        return quantity;
    }
}
