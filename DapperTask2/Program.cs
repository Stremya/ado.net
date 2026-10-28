using Dapper;
using Microsoft.Data.SqlClient;

namespace NewsletterApp;
public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty; 
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;     
    public string CountryName { get; set; } = string.Empty;  
}

public class Promotion
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty; 
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;  
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class NewsletterService
{
    const string MasterConn =
        @"Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
    const string DbConn =
        @"Server=localhost;Database=NewsletterDb;Trusted_Connection=True;TrustServerCertificate=True;";

    private SqlConnection? _connection;

    public bool IsConnected => _connection != null && _connection.State == System.Data.ConnectionState.Open;

    public bool Connect()
    {
        try
        {
            _connection?.Dispose();
            _connection = new SqlConnection(DbConn);
            _connection.Open();
            _connection.Execute("SELECT 1"); 
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ОШИБКА подключения: " + ex.Message);
            _connection?.Dispose();
            _connection = null;
            return false;
        }
    }

    public void Disconnect()
    {
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
            Console.WriteLine("Отключено от базы данных.");
        }
        else Console.WriteLine("Соединение уже закрыто.");
    }

    private SqlConnection Conn =>
        IsConnected ? _connection! : throw new InvalidOperationException("Нет подключения к базе.");

    public void EnsureDatabaseExists()
    {
        using (var master = new SqlConnection(MasterConn))
            master.Execute("IF DB_ID('NewsletterDb') IS NULL EXEC('CREATE DATABASE NewsletterDb');");

        using (var db = new SqlConnection(DbConn))
        {
            db.Execute(@"
IF OBJECT_ID('dbo.Countries','U') IS NULL CREATE TABLE dbo.Countries(Id INT PRIMARY KEY IDENTITY,Name NVARCHAR(100));
IF OBJECT_ID('dbo.Categories','U') IS NULL CREATE TABLE dbo.Categories(Id INT PRIMARY KEY IDENTITY,Name NVARCHAR(100));
IF OBJECT_ID('dbo.Cities','U') IS NULL CREATE TABLE dbo.Cities(Id INT PRIMARY KEY IDENTITY,Name NVARCHAR(100),CountryId INT NOT NULL REFERENCES dbo.Countries(Id));
IF OBJECT_ID('dbo.Customers','U') IS NULL CREATE TABLE dbo.Customers(Id INT PRIMARY KEY IDENTITY,Name NVARCHAR(100),BirthDate DATE,Gender NVARCHAR(20),Email NVARCHAR(100),CityId INT NOT NULL REFERENCES dbo.Cities(Id));
IF OBJECT_ID('dbo.CustomerCategories','U') IS NULL CREATE TABLE dbo.CustomerCategories(CustomerId INT NOT NULL REFERENCES dbo.Customers(Id),CategoryId INT NOT NULL REFERENCES dbo.Categories(Id),PRIMARY KEY(CustomerId,CategoryId));
IF OBJECT_ID('dbo.Promotions','U') IS NULL CREATE TABLE dbo.Promotions(Id INT PRIMARY KEY IDENTITY,ProductName NVARCHAR(150),CategoryId INT NOT NULL REFERENCES dbo.Categories(Id),CountryId INT NOT NULL REFERENCES dbo.Countries(Id),StartDate DATE,EndDate DATE);");

            if (db.QueryFirstOrDefault<int>("SELECT COUNT(1) FROM dbo.Countries") == 0)
                Seed(db);
        }
    }

    private void Seed(SqlConnection db)
    {
        db.Execute(@"
SET IDENTITY_INSERT dbo.Countries ON;
INSERT INTO dbo.Countries(Id,Name) VALUES(1,N'Россия'),(2,N'Германия'),(3,N'Испания');
SET IDENTITY_INSERT dbo.Countries OFF;

SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories(Id,Name) VALUES(1,N'Мобильные телефоны'),(2,N'Ноутбуки'),(3,N'Кухонная техника');
SET IDENTITY_INSERT dbo.Categories OFF;

SET IDENTITY_INSERT dbo.Cities ON;
INSERT INTO dbo.Cities(Id,Name,CountryId) VALUES
(1,N'Москва',1),(2,N'Санкт-Петербург',1),(3,N'Берлин',2),(4,N'Мюнхен',2),(5,N'Мадрид',3),(6,N'Барселона',3);
SET IDENTITY_INSERT dbo.Cities OFF;

SET IDENTITY_INSERT dbo.Customers ON;
INSERT INTO dbo.Customers(Id,Name,BirthDate,Gender,Email,CityId) VALUES
(1,N'Анна Смирнова','1995-03-12',N'Женский',N'anna@mail.ru',1),
(2,N'Борис Иванов','1988-07-25',N'Мужской',N'boris@mail.ru',2),
(3,N'Грета Мюллер','1990-11-02',N'Женский',N'greta@mail.de',3),
(4,N'Ханс Шмидт','1985-01-30',N'Мужской',N'hans@mail.de',4),
(5,N'Карлос Гомес','1992-09-18',N'Мужской',N'carlos@mail.es',5),
(6,N'Мария Лопес','1998-05-05',N'Женский',N'maria@mail.es',6);
SET IDENTITY_INSERT dbo.Customers OFF;

INSERT INTO dbo.CustomerCategories(CustomerId,CategoryId) VALUES
(1,1),(1,2),(2,2),(3,1),(3,3),(4,2),(5,1),(5,3),(6,3);

SET IDENTITY_INSERT dbo.Promotions ON;
INSERT INTO dbo.Promotions(Id,ProductName,CategoryId,CountryId,StartDate,EndDate) VALUES
(1,N'iPhone 15 Pro',1,1,'2026-10-01','2026-11-01'),
(2,N'Samsung Galaxy S24',1,1,'2026-10-15','2026-10-31'),
(3,N'MacBook Air M3',2,2,'2026-10-10','2026-10-25'),
(4,N'ASUS ROG Laptop',2,3,'2026-10-05','2026-11-05'),
(5,N'Bosch Coffee Machine',3,2,'2026-10-20','2026-11-10'),
(6,N'LG Холодильник',3,3,'2026-10-01','2026-10-30');
SET IDENTITY_INSERT dbo.Promotions OFF;");
    }

    public List<Customer> GetAllCustomers() => Conn.Query<Customer>(@"
        SELECT c.Id,c.Name,c.BirthDate,c.Gender,c.Email,c.CityId,ci.Name AS CityName,co.Name AS CountryName
        FROM dbo.Customers c JOIN dbo.Cities ci ON c.CityId=ci.Id JOIN dbo.Countries co ON ci.CountryId=co.Id
        ORDER BY c.Name").ToList();

    public List<string> GetAllEmails() =>
        Conn.Query<string>("SELECT Email FROM dbo.Customers ORDER BY Email").ToList();

    public List<Category> GetAllCategories() =>
        Conn.Query<Category>("SELECT Id,Name FROM dbo.Categories ORDER BY Name").ToList();

    public List<Promotion> GetAllPromotions() => Conn.Query<Promotion>(@"
        SELECT p.Id,p.ProductName,p.CategoryId,cat.Name AS CategoryName,p.CountryId,co.Name AS CountryName,p.StartDate,p.EndDate
        FROM dbo.Promotions p JOIN dbo.Categories cat ON p.CategoryId=cat.Id JOIN dbo.Countries co ON p.CountryId=co.Id
        ORDER BY co.Name,cat.Name").ToList();

    public List<City> GetAllCities() => Conn.Query<City>(@"
        SELECT ci.Id,ci.Name,ci.CountryId,co.Name AS CountryName
        FROM dbo.Cities ci JOIN dbo.Countries co ON ci.CountryId=co.Id ORDER BY co.Name,ci.Name").ToList();

    public List<Country> GetAllCountries() =>
        Conn.Query<Country>("SELECT Id,Name FROM dbo.Countries ORDER BY Name").ToList();

    public List<Customer> GetCustomersByCity(string cityName) => Conn.Query<Customer>(@"
        SELECT c.Id,c.Name,c.BirthDate,c.Gender,c.Email,c.CityId,ci.Name AS CityName,co.Name AS CountryName
        FROM dbo.Customers c JOIN dbo.Cities ci ON c.CityId=ci.Id JOIN dbo.Countries co ON ci.CountryId=co.Id
        WHERE ci.Name=@CityName ORDER BY c.Name", new { CityName = cityName }).ToList();

    public List<Customer> GetCustomersByCountry(string countryName) => Conn.Query<Customer>(@"
        SELECT c.Id,c.Name,c.BirthDate,c.Gender,c.Email,c.CityId,ci.Name AS CityName,co.Name AS CountryName
        FROM dbo.Customers c JOIN dbo.Cities ci ON c.CityId=ci.Id JOIN dbo.Countries co ON ci.CountryId=co.Id
        WHERE co.Name=@CountryName ORDER BY c.Name", new { CountryName = countryName }).ToList();

    public List<Promotion> GetPromotionsByCountry(string countryName) => Conn.Query<Promotion>(@"
        SELECT p.Id,p.ProductName,p.CategoryId,cat.Name AS CategoryName,p.CountryId,co.Name AS CountryName,p.StartDate,p.EndDate
        FROM dbo.Promotions p JOIN dbo.Categories cat ON p.CategoryId=cat.Id JOIN dbo.Countries co ON p.CountryId=co.Id
        WHERE co.Name=@CountryName ORDER BY cat.Name", new { CountryName = countryName }).ToList();
}

class Program
{
    static void Main()
    {
        var svc = new NewsletterService();
        Console.OutputEncoding = System.Text.Encoding.UTF8; 
        Console.WriteLine("Инициализация базы данных (создание таблиц и тестовых данных)...");
        svc.EnsureDatabaseExists();
        Console.WriteLine("База готова.\n");

        while (true)
        {
            Console.WriteLine("================ МЕНЮ ================");
            Console.WriteLine("1. Подключиться к базе");
            Console.WriteLine("2. Отключиться от базы");
            Console.WriteLine("3. Все покупатели");
            Console.WriteLine("4. Email всех покупателей");
            Console.WriteLine("5. Список разделов");
            Console.WriteLine("6. Список акционных товаров");
            Console.WriteLine("7. Все города");
            Console.WriteLine("8. Все страны");
            Console.WriteLine("9. Покупатели из города");
            Console.WriteLine("10. Покупатели из страны");
            Console.WriteLine("11. Акции для страны");
            Console.WriteLine("0. Выход");
            Console.Write("Выбор: ");
            var cmd = Console.ReadLine();

            if (int.TryParse(cmd, out int choice) && choice >= 3 && choice <= 11 && !svc.IsConnected)
            {
                Console.WriteLine(">> Сначала подключитесь к базе (пункт 1).\n");
                continue;
            }

            switch (cmd)
            {
                case "1":
                    Console.WriteLine(svc.Connect() ? ">> Подключение успешно выполнено." : ">> Подключение НЕ удалось.");
                    break;
                case "2":
                    svc.Disconnect();
                    break;
                case "3":
                    PrintCustomers(svc.GetAllCustomers());
                    break;
                case "4":
                    Console.WriteLine("Email-адреса покупателей:");
                    foreach (var e in svc.GetAllEmails()) Console.WriteLine("  " + e);
                    break;
                case "5":
                    Console.WriteLine("Разделы товаров:");
                    foreach (var c in svc.GetAllCategories()) Console.WriteLine($"  [{c.Id}] {c.Name}");
                    break;
                case "6":
                    PrintPromotions(svc.GetAllPromotions());
                    break;
                case "7":
                    Console.WriteLine("Города:");
                    foreach (var c in svc.GetAllCities()) Console.WriteLine($"  {c.Name} ({c.CountryName})");
                    break;
                case "8":
                    Console.WriteLine("Страны:");
                    foreach (var c in svc.GetAllCountries()) Console.WriteLine($"  {c.Name}");
                    break;
                case "9":
                    Console.Write("Введите название города: ");
                    PrintCustomers(svc.GetCustomersByCity(Console.ReadLine()!.Trim()));
                    break;
                case "10":
                    Console.Write("Введите название страны: ");
                    PrintCustomers(svc.GetCustomersByCountry(Console.ReadLine()!.Trim()));
                    break;
                case "11":
                    Console.Write("Введите название страны: ");
                    PrintPromotions(svc.GetPromotionsByCountry(Console.ReadLine()!.Trim()));
                    break;
                case "0":
                    svc.Disconnect();
                    Console.WriteLine("Выход. Пока!");
                    return;
                default:
                    Console.WriteLine("Неизвестная команда.");
                    break;
            }
            Console.WriteLine();
        }
    }

    static void PrintCustomers(List<Customer> list)
    {
        if (list.Count == 0) { Console.WriteLine("Никого не найдено."); return; }
        Console.WriteLine($"Покупателей: {list.Count}");
        foreach (var c in list)
            Console.WriteLine($"  {c.Name} | {c.Gender} | {c.BirthDate:d} | {c.Email} | {c.CityName}, {c.CountryName}");
    }

    static void PrintPromotions(List<Promotion> list)
    {
        if (list.Count == 0) { Console.WriteLine("Акции не найдены."); return; }
        Console.WriteLine($"Акций: {list.Count}");
        foreach (var p in list)
            Console.WriteLine($"  {p.ProductName} [{p.CategoryName}] | {p.CountryName} | {p.StartDate:d} — {p.EndDate:d}");
    }
}