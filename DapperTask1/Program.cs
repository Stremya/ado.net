using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DapperExample;

// Класс-модель (POCO). Имена свойств совпадают с колонками таблицы
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

class Program
{
    // Строка подключения к нашей базе TestDB
    static string connectionString =
        @"Server=localhost;Database=TestDB;Trusted_Connection=True;TrustServerCertificate=True;";

    static void Main()
    {
        // ========== 1. ВСТАВКА (INSERT) ==========
        var newUser = new User { Name = "Иван Петров", Email = "ivan@example.com", Age = 30 };
        string insertSql = "INSERT INTO Users (Name, Email, Age) VALUES (@Name, @Email, @Age)";

        using (IDbConnection db = new SqlConnection(connectionString))
        {
            int rowsAffected = db.Execute(insertSql, newUser);
            Console.WriteLine($"Вставлено строк: {rowsAffected}");
        }

        // ========== 2. ЧТЕНИЕ (SELECT) ==========
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            // Получаем всех пользователей старше 25 лет
            var users = db.Query<User>("SELECT * FROM Users WHERE Age > @Age", new { Age = 25 }).ToList();

            Console.WriteLine("Пользователи старше 25:");
            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id}, Имя: {user.Name}, Email: {user.Email}, Возраст: {user.Age}");
            }
        }

        // ========== 3. ОБНОВЛЕНИЕ (UPDATE) ==========
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            string updateSql = "UPDATE Users SET Age = @Age WHERE Name = @Name";
            int updated = db.Execute(updateSql, new { Age = 31, Name = "Иван Петров" });
            Console.WriteLine($"Обновлено строк: {updated}");
        }

        // ========== 4. УДАЛЕНИЕ (DELETE) ==========
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            string deleteSql = "DELETE FROM Users WHERE Name = @Name";
            int deleted = db.Execute(deleteSql, new { Name = "Иван Петров" });
            Console.WriteLine($"Удалено строк: {deleted}");
        }

        // ========== 5. ПРОВЕРКА через QueryFirstOrDefault ==========
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            var check = db.QueryFirstOrDefault<User>(
                "SELECT * FROM Users WHERE Name = @Name",
                new { Name = "Иван Петров" });

            Console.WriteLine(check == null
                ? "Проверка: пользователь удалён из базы"
                : "Проверка: пользователь всё ещё в базе");
        }

        Console.WriteLine("\nГотово! Нажми любую клавишу...");
        Console.ReadKey();
    }
}