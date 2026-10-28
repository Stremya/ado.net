using System.Data;                 
using Dapper;
using Microsoft.Data.SqlClient;

namespace NewsletterApp;

public class Country { public int Id { get; set; } public string Name { get; set; } = ""; }
public class City { public int Id { get; set; } public string Name { get; set; } = ""; public int CountryId { get; set; } public string CountryName { get; set; } = ""; }
public class Category { public int Id { get; set; } public string Name { get; set; } = ""; }

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = "";
    public string Email { get; set; } = "";
    public int CityId { get; set; }
    public string CityName { get; set; } = "";
    public string CountryName { get; set; } = "";
}

public class Promotion
{
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int CountryId { get; set; }
    public string CountryName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CityStat { public string CityName { get; set; } = ""; public string CountryName { get; set; } = ""; public int CustomersCount { get; set; } }
public class CountryStat { public string CountryName { get; set; } = ""; public int CustomersCount { get; set; } }
public class CitiesPerCountryStat { public string CountryName { get; set; } = ""; public int CitiesCount { get; set; } }

public class NewsletterService
{
    const string MasterConn = @"Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
    const string DbConn = @"Server=localhost;Database=NewsletterDb;Trusted_Connection=True;TrustServerCertificate=True;";

    private SqlConnection? _connection;
    public bool IsConnected => _connection != null && _connection.State == ConnectionState.Open;

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _connection?.Dispose();
            _connection = new SqlConnection(DbConn);
            await _connection.OpenAsync();
            await _connection.ExecuteAsync("SELECT 1");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ОШИБКА подключения: " + ex.Message);
            _connection?.Dispose(); _connection = null;
            return false;
        }
    }

    public void Disconnect()
    {
        if (_connection != null) { _connection.Close(); _connection.Dispose(); _connection = null; Console.WriteLine("Отключено от базы данных."); }
        else Console.WriteLine("Соединение уже закрыто.");
    }

    private SqlConnection Conn => IsConnected ? _connection! : throw new InvalidOperationException("Нет подключения к базе.");

    public async Task EnsureDatabaseExistsAsync()
    {
        using (var master = new SqlConnection(MasterConn))
            await master.ExecuteAsync("IF DB_ID('NewsletterDb') IS NULL EXEC('CREATE DATABASE NewsletterDb');");

        using (var db = new SqlConnection(DbConn))
        {
            await db.ExecuteAsync(@"
IF OBJECT_ID('dbo.Countries','U') IS NULL CREATE TABLE dbo.Countries(Id INT PRIMARY KEY IDENTITY,Name NVARCHAR(100));
IF OBJECT_ID('dbo.Categories','U') IS NULL CREATE TABLE dbo.Categories(Id INT PRIMARY KEY IDENTITY,Name NVARCHAR(100));
IF OBJECT_ID('dbo.Cities','U') IS NULL CREATE TABLE dbo.Cities(Id INT PRIMARY KEY IDENTITY,Name NVARCHAR(100),CountryId INT NOT NULL REFERENCES dbo.Countries(Id));
IF OBJECT_ID('dbo.Customers','U') IS NULL CREATE TABLE dbo.Customers(Id INT PRIMARY KEY IDENTITY,Name NVARCHAR(100),BirthDate DATE,Gender NVARCHAR(20),Email NVARCHAR(100),CityId INT NOT NULL REFERENCES dbo.Cities(Id));
IF OBJECT_ID('dbo.CustomerCategories','U') IS NULL CREATE TABLE dbo.CustomerCategories(CustomerId INT NOT NULL REFERENCES dbo.Customers(Id),CategoryId INT NOT NULL REFERENCES dbo.Categories(Id),PRIMARY KEY(CustomerId,CategoryId));
IF OBJECT_ID('dbo.Promotions','U') IS NULL CREATE TABLE dbo.Promotions(Id INT PRIMARY KEY IDENTITY,ProductName NVARCHAR(150),CategoryId INT NOT NULL REFERENCES dbo.Categories(Id),CountryId INT NOT NULL REFERENCES dbo.Countries(Id),StartDate DATE,EndDate DATE);");

            if (await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT(1) FROM dbo.Countries") == 0)
                await SeedAsync(db);

            await CreateDbObjectsAsync(db);   
        }
    }

    private async Task SeedAsync(SqlConnection db)
    {
        await db.ExecuteAsync(@"
SET IDENTITY_INSERT dbo.Countries ON;
INSERT INTO dbo.Countries(Id,Name) VALUES(1,N'Россия'),(2,N'Германия'),(3,N'Испания');
SET IDENTITY_INSERT dbo.Countries OFF;
SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories(Id,Name) VALUES(1,N'Мобильные телефоны'),(2,N'Ноутбуки'),(3,N'Кухонная техника');
SET IDENTITY_INSERT dbo.Categories OFF;
SET IDENTITY_INSERT dbo.Cities ON;
INSERT INTO dbo.Cities(Id,Name,CountryId) VALUES(1,N'Москва',1),(2,N'Санкт-Петербург',1),(3,N'Берлин',2),(4,N'Мюнхен',2),(5,N'Мадрид',3),(6,N'Барселона',3);
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
INSERT INTO dbo.CustomerCategories(CustomerId,CategoryId) VALUES(1,1),(1,2),(2,2),(3,1),(3,3),(4,2),(5,1),(5,3),(6,3);
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

    private async Task CreateDbObjectsAsync(SqlConnection db)
    {
        await db.ExecuteAsync(@"CREATE OR ALTER VIEW dbo.vw_CustomersPerCity AS
SELECT ci.Id CityId, ci.Name CityName, co.Name CountryName, COUNT(c.Id) CustomersCount
FROM dbo.Cities ci JOIN dbo.Countries co ON ci.CountryId=co.Id
LEFT JOIN dbo.Customers c ON c.CityId=ci.Id
GROUP BY ci.Id, ci.Name, co.Name;");

        await db.ExecuteAsync(@"CREATE OR ALTER VIEW dbo.vw_CustomersPerCountry AS
SELECT co.Id CountryId, co.Name CountryName, COUNT(c.Id) CustomersCount
FROM dbo.Countries co LEFT JOIN dbo.Cities ci ON ci.CountryId=co.Id
LEFT JOIN dbo.Customers c ON c.CityId=ci.Id
GROUP BY co.Id, co.Name;");

        await db.ExecuteAsync(@"CREATE OR ALTER VIEW dbo.vw_CitiesPerCountry AS
SELECT co.Id CountryId, co.Name CountryName, COUNT(ci.Id) CitiesCount
FROM dbo.Countries co LEFT JOIN dbo.Cities ci ON ci.CountryId=co.Id
GROUP BY co.Id, co.Name;");

        await db.ExecuteAsync(@"CREATE OR ALTER FUNCTION dbo.fn_AvgCitiesPerCountry()
RETURNS DECIMAL(10,2) AS
BEGIN
    DECLARE @avg DECIMAL(10,2);
    SELECT @avg = AVG(Cnt) FROM (
        SELECT COUNT(ci.Id) Cnt FROM dbo.Countries co LEFT JOIN dbo.Cities ci ON ci.CountryId=co.Id GROUP BY co.Id
    ) t;
    RETURN ISNULL(@avg,0);
END;");

        await db.ExecuteAsync(@"CREATE OR ALTER PROCEDURE dbo.sp_SectionsByCountryCustomers @CountryName NVARCHAR(100) AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT cat.Id, cat.Name FROM dbo.Categories cat
    JOIN dbo.CustomerCategories cc ON cc.CategoryId=cat.Id
    JOIN dbo.Customers c ON c.Id=cc.CustomerId
    JOIN dbo.Cities ci ON c.CityId=ci.Id
    JOIN dbo.Countries co ON co.Id=ci.CountryId
    WHERE co.Name=@CountryName ORDER BY cat.Name;
END;");

        await db.ExecuteAsync(@"CREATE OR ALTER PROCEDURE dbo.sp_PromotionsBySectionInPeriod @CategoryName NVARCHAR(100), @FromDate DATE, @ToDate DATE AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id, p.ProductName, p.CategoryId, cat.Name AS CategoryName, p.CountryId, co.Name AS CountryName, p.StartDate, p.EndDate
    FROM dbo.Promotions p JOIN dbo.Categories cat ON p.CategoryId=cat.Id JOIN dbo.Countries co ON p.CountryId=co.Id
    WHERE cat.Name=@CategoryName AND p.StartDate<=@ToDate AND p.EndDate>=@FromDate   -- акции, активные в периоде
    ORDER BY p.StartDate;
END;");

        await db.ExecuteAsync(@"CREATE OR ALTER PROCEDURE dbo.sp_PromotionsForCustomer @CustomerName NVARCHAR(100) AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id, p.ProductName, p.CategoryId, cat.Name AS CategoryName, p.CountryId, co.Name AS CountryName, p.StartDate, p.EndDate
    FROM dbo.Promotions p
    JOIN dbo.Categories cat ON p.CategoryId=cat.Id
    JOIN dbo.Countries co ON p.CountryId=co.Id
    JOIN dbo.Customers c ON c.Name=@CustomerName
    JOIN dbo.Cities ci ON c.CityId=ci.Id
    WHERE p.CountryId=ci.CountryId
      AND p.CategoryId IN (SELECT cc.CategoryId FROM dbo.CustomerCategories cc WHERE cc.CustomerId=c.Id)
    ORDER BY cat.Name;
END;");

        await db.ExecuteAsync(@"CREATE OR ALTER TRIGGER dbo.trg_Promotions_DateRange ON dbo.Promotions
AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS(SELECT 1 FROM inserted WHERE EndDate < StartDate)
        THROW 50001, N'Дата конца акции не может быть раньше даты начала.', 1;
END;");
    }

    public async Task<List<Customer>> GetAllCustomersAsync() => (await Conn.QueryAsync<Customer>(@"
        SELECT c.Id,c.Name,c.BirthDate,c.Gender,c.Email,c.CityId,ci.Name AS CityName,co.Name AS CountryName
        FROM dbo.Customers c JOIN dbo.Cities ci ON c.CityId=ci.Id JOIN dbo.Countries co ON ci.CountryId=co.Id ORDER BY c.Name")).ToList();

    public async Task<List<string>> GetAllEmailsAsync() => (await Conn.QueryAsync<string>("SELECT Email FROM dbo.Customers ORDER BY Email")).ToList();
    public async Task<List<Category>> GetAllCategoriesAsync() => (await Conn.QueryAsync<Category>("SELECT Id,Name FROM dbo.Categories ORDER BY Name")).ToList();
    public async Task<List<Promotion>> GetAllPromotionsAsync() => (await Conn.QueryAsync<Promotion>(@"
        SELECT p.Id,p.ProductName,p.CategoryId,cat.Name AS CategoryName,p.CountryId,co.Name AS CountryName,p.StartDate,p.EndDate
        FROM dbo.Promotions p JOIN dbo.Categories cat ON p.CategoryId=cat.Id JOIN dbo.Countries co ON p.CountryId=co.Id ORDER BY co.Name,cat.Name")).ToList();
    public async Task<List<City>> GetAllCitiesAsync() => (await Conn.QueryAsync<City>(@"
        SELECT ci.Id,ci.Name,ci.CountryId,co.Name AS CountryName FROM dbo.Cities ci JOIN dbo.Countries co ON ci.CountryId=co.Id ORDER BY co.Name,ci.Name")).ToList();
    public async Task<List<Country>> GetAllCountriesAsync() => (await Conn.QueryAsync<Country>("SELECT Id,Name FROM dbo.Countries ORDER BY Name")).ToList();

    public async Task<List<Customer>> GetCustomersByCityAsync(string cityName) => (await Conn.QueryAsync<Customer>(@"
        SELECT c.Id,c.Name,c.BirthDate,c.Gender,c.Email,c.CityId,ci.Name AS CityName,co.Name AS CountryName
        FROM dbo.Customers c JOIN dbo.Cities ci ON c.CityId=ci.Id JOIN dbo.Countries co ON ci.CountryId=co.Id
        WHERE ci.Name=@CityName ORDER BY c.Name", new { CityName = cityName })).ToList();

    public async Task<List<Customer>> GetCustomersByCountryAsync(string countryName) => (await Conn.QueryAsync<Customer>(@"
        SELECT c.Id,c.Name,c.BirthDate,c.Gender,c.Email,c.CityId,ci.Name AS CityName,co.Name AS CountryName
        FROM dbo.Customers c JOIN dbo.Cities ci ON c.CityId=ci.Id JOIN dbo.Countries co ON ci.CountryId=co.Id
        WHERE co.Name=@CountryName ORDER BY c.Name", new { CountryName = countryName })).ToList();

    public async Task<List<Promotion>> GetPromotionsByCountryAsync(string countryName) => (await Conn.QueryAsync<Promotion>(@"
        SELECT p.Id,p.ProductName,p.CategoryId,cat.Name AS CategoryName,p.CountryId,co.Name AS CountryName,p.StartDate,p.EndDate
        FROM dbo.Promotions p JOIN dbo.Categories cat ON p.CategoryId=cat.Id JOIN dbo.Countries co ON p.CountryId=co.Id
        WHERE co.Name=@CountryName ORDER BY cat.Name", new { CountryName = countryName })).ToList();

    public async Task<List<CityStat>> GetCustomersPerCityAsync() =>
        (await Conn.QueryAsync<CityStat>("SELECT CityName,CountryName,CustomersCount FROM dbo.vw_CustomersPerCity ORDER BY CustomersCount DESC, CityName")).ToList();

    public async Task<List<CountryStat>> GetCustomersPerCountryAsync() =>
        (await Conn.QueryAsync<CountryStat>("SELECT CountryName,CustomersCount FROM dbo.vw_CustomersPerCountry ORDER BY CustomersCount DESC, CountryName")).ToList();

    public async Task<List<CitiesPerCountryStat>> GetCitiesPerCountryAsync() =>
        (await Conn.QueryAsync<CitiesPerCountryStat>("SELECT CountryName,CitiesCount FROM dbo.vw_CitiesPerCountry ORDER BY CitiesCount DESC, CountryName")).ToList();

    public async Task<decimal> GetAvgCitiesPerCountryAsync() =>
        await Conn.QueryFirstOrDefaultAsync<decimal>("SELECT dbo.fn_AvgCitiesPerCountry()");

    public async Task<List<Category>> GetSectionsByCountryCustomersAsync(string countryName) =>
        (await Conn.QueryAsync<Category>("dbo.sp_SectionsByCountryCustomers", new { CountryName = countryName }, commandType: CommandType.StoredProcedure)).ToList();

    public async Task<List<Promotion>> GetPromotionsBySectionInPeriodAsync(string categoryName, DateTime from, DateTime to) =>
        (await Conn.QueryAsync<Promotion>("dbo.sp_PromotionsBySectionInPeriod", new { CategoryName = categoryName, FromDate = from, ToDate = to }, commandType: CommandType.StoredProcedure)).ToList();

    public async Task<List<Promotion>> GetPromotionsForCustomerAsync(string customerName) =>
        (await Conn.QueryAsync<Promotion>("dbo.sp_PromotionsForCustomer", new { CustomerName = customerName }, commandType: CommandType.StoredProcedure)).ToList();

    public async Task<List<CountryStat>> GetTopCountriesByCustomersAsync(int count) =>
        (await Conn.QueryAsync<CountryStat>("SELECT TOP(@Count) CountryName,CustomersCount FROM dbo.vw_CustomersPerCountry ORDER BY CustomersCount DESC", new { Count = count })).ToList();

    public async Task<CountryStat?> GetBestCountryByCustomersAsync() =>
        await Conn.QueryFirstOrDefaultAsync<CountryStat>("SELECT TOP(1) CountryName,CustomersCount FROM dbo.vw_CustomersPerCountry ORDER BY CustomersCount DESC");

    public async Task<List<CityStat>> GetTopCitiesByCustomersAsync(int count) =>
        (await Conn.QueryAsync<CityStat>("SELECT TOP(@Count) CityName,CountryName,CustomersCount FROM dbo.vw_CustomersPerCity ORDER BY CustomersCount DESC", new { Count = count })).ToList();

    public async Task<CityStat?> GetBestCityByCustomersAsync() =>
        await Conn.QueryFirstOrDefaultAsync<CityStat>("SELECT TOP(1) CityName,CountryName,CustomersCount FROM dbo.vw_CustomersPerCity ORDER BY CustomersCount DESC");

    public async Task<string> TryAddPromotionAsync(string product, string category, string country, DateTime start, DateTime end)
    {
        try
        {
            await Conn.ExecuteAsync(@"
INSERT INTO dbo.Promotions(ProductName,CategoryId,CountryId,StartDate,EndDate)
SELECT @Product, (SELECT Id FROM dbo.Categories WHERE Name=@Category), (SELECT Id FROM dbo.Countries WHERE Name=@Country), @Start, @End;",
                new { Product = product, Category = category, Country = country, Start = start, End = end });
            return "OK";
        }
        catch (SqlException ex) when (ex.Number == 50001 || ex.Message.Contains("акци"))
        {
            return "TRIGGER: " + ex.Message;
        }
    }
}

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var svc = new NewsletterService();
        Console.WriteLine("Инициализация базы (таблицы + представления + функция + процедуры + триггер)...");
        await svc.EnsureDatabaseExistsAsync();
        Console.WriteLine("База готова.\n");

        while (true)
        {
            Console.WriteLine("================ МЕНЮ ================");
            Console.WriteLine(" 1. Подключиться    2. Отключиться");
            Console.WriteLine(" 3. Все покупатели  4. Email всех    5. Разделы   6. Все акции");
            Console.WriteLine(" 7. Все города      8. Все страны    9. Покупатели города  10. Покупатели страны  11. Акции страны");
            Console.WriteLine("--- Задание 1 (агрегаты) ---");
            Console.WriteLine("12. Покупателей в каждом городе   13. Покупателей в каждой стране");
            Console.WriteLine("14. Городов в каждой стране       15. Среднее городов по странам (функция)");
            Console.WriteLine("--- Задание 2 (процедуры) ---");
            Console.WriteLine("16. Разделы покупателей страны    17. Акции раздела за период   18. Акции для покупателя");
            Console.WriteLine("--- Задание 3 (топы) ---");
            Console.WriteLine("19. Топ-3 стран по покупателям    20. Лучшая страна");
            Console.WriteLine("21. Топ-3 городов по покупателям  22. Лучший город");
            Console.WriteLine("--- Триггер ---");
            Console.WriteLine("23. Добавить акцию (проверка триггером дат)");
            Console.WriteLine(" 0. Выход");
            Console.Write("Выбор: ");
            var cmd = Console.ReadLine();

            if (int.TryParse(cmd, out int choice) && choice >= 3 && choice <= 23 && !svc.IsConnected)
            {
                Console.WriteLine(">> Сначала подключитесь к базе (пункт 1).\n");
                continue;
            }

            switch (cmd)
            {
                case "1": Console.WriteLine(await svc.ConnectAsync() ? ">> Подключение успешно." : ">> Подключение НЕ удалось."); break;
                case "2": svc.Disconnect(); break;
                case "3": PrintCustomers(await svc.GetAllCustomersAsync()); break;
                case "4": foreach (var e in await svc.GetAllEmailsAsync()) Console.WriteLine("  " + e); break;
                case "5": foreach (var c in await svc.GetAllCategoriesAsync()) Console.WriteLine($"  [{c.Id}] {c.Name}"); break;
                case "6": PrintPromotions(await svc.GetAllPromotionsAsync()); break;
                case "7": foreach (var c in await svc.GetAllCitiesAsync()) Console.WriteLine($"  {c.Name} ({c.CountryName})"); break;
                case "8": foreach (var c in await svc.GetAllCountriesAsync()) Console.WriteLine("  " + c.Name); break;
                case "9": Console.Write("Город: "); PrintCustomers(await svc.GetCustomersByCityAsync(Console.ReadLine()!.Trim())); break;
                case "10": Console.Write("Страна: "); PrintCustomers(await svc.GetCustomersByCountryAsync(Console.ReadLine()!.Trim())); break;
                case "11": Console.Write("Страна: "); PrintPromotions(await svc.GetPromotionsByCountryAsync(Console.ReadLine()!.Trim())); break;

                case "12": Console.WriteLine("Покупателей в каждом городе:"); foreach (var s in await svc.GetCustomersPerCityAsync()) Console.WriteLine($"  {s.CityName} ({s.CountryName}): {s.CustomersCount}"); break;
                case "13": Console.WriteLine("Покупателей в каждой стране:"); foreach (var s in await svc.GetCustomersPerCountryAsync()) Console.WriteLine($"  {s.CountryName}: {s.CustomersCount}"); break;
                case "14": Console.WriteLine("Городов в каждой стране:"); foreach (var s in await svc.GetCitiesPerCountryAsync()) Console.WriteLine($"  {s.CountryName}: {s.CitiesCount}"); break;
                case "15": Console.WriteLine($"Среднее количество городов по странам (fn_AvgCitiesPerCountry): {await svc.GetAvgCitiesPerCountryAsync():0.##}"); break;

                case "16": Console.Write("Страна: "); foreach (var c in await svc.GetSectionsByCountryCustomersAsync(Console.ReadLine()!.Trim())) Console.WriteLine($"  {c.Name}"); break;
                case "17":
                    Console.Write("Раздел: "); var cat = Console.ReadLine()!.Trim();
                    Console.Write("С (ГГГГ-ММ-ДД): "); var f = DateTime.Parse(Console.ReadLine()!.Trim());
                    Console.Write("По (ГГГГ-ММ-ДД): "); var t = DateTime.Parse(Console.ReadLine()!.Trim());
                    PrintPromotions(await svc.GetPromotionsBySectionInPeriodAsync(cat, f, t)); break;
                case "18": Console.Write("Имя покупателя: "); PrintPromotions(await svc.GetPromotionsForCustomerAsync(Console.ReadLine()!.Trim())); break;

                case "19": Console.WriteLine("Топ-3 стран:"); foreach (var s in await svc.GetTopCountriesByCustomersAsync(3)) Console.WriteLine($"  {s.CountryName}: {s.CustomersCount}"); break;
                case "20": var bc = await svc.GetBestCountryByCustomersAsync(); Console.WriteLine($"Лучшая страна: {bc?.CountryName} ({bc?.CustomersCount})"); break;
                case "21": Console.WriteLine("Топ-3 городов:"); foreach (var s in await svc.GetTopCitiesByCustomersAsync(3)) Console.WriteLine($"  {s.CityName} ({s.CountryName}): {s.CustomersCount}"); break;
                case "22": var bp = await svc.GetBestCityByCustomersAsync(); Console.WriteLine($"Лучший город: {bp?.CityName} ({bp?.CountryName}): {bp?.CustomersCount}"); break;

                case "23":
                    Console.Write("Товар: "); var pr = Console.ReadLine()!.Trim();
                    Console.Write("Раздел: "); var pc = Console.ReadLine()!.Trim();
                    Console.Write("Страна: "); var py = Console.ReadLine()!.Trim();
                    Console.Write("Начало (ГГГГ-ММ-ДД): "); var ps = DateTime.Parse(Console.ReadLine()!.Trim());
                    Console.Write("Конец  (ГГГГ-ММ-ДД): "); var pe = DateTime.Parse(Console.ReadLine()!.Trim());
                    var res = await svc.TryAddPromotionAsync(pr, pc, py, ps, pe);
                    Console.WriteLine(res == "OK" ? ">> Акция добавлена." : ">> " + res);
                    break;

                case "0": svc.Disconnect(); Console.WriteLine("Выход. Пока!"); return;
                default: Console.WriteLine("Неизвестная команда."); break;
            }
            Console.WriteLine();
        }
    }

    static void PrintCustomers(List<Customer> list)
    {
        if (list.Count == 0) { Console.WriteLine("Никого не найдено."); return; }
        Console.WriteLine($"Покупателей: {list.Count}");
        foreach (var c in list) Console.WriteLine($"  {c.Name} | {c.Gender} | {c.BirthDate:d} | {c.Email} | {c.CityName}, {c.CountryName}");
    }

    static void PrintPromotions(List<Promotion> list)
    {
        if (list.Count == 0) { Console.WriteLine("Акции не найдены."); return; }
        Console.WriteLine($"Акций: {list.Count}");
        foreach (var p in list) Console.WriteLine($"  {p.ProductName} [{p.CategoryName}] | {p.CountryName} | {p.StartDate:d} — {p.EndDate:d}");
    }
}