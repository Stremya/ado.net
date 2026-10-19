using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks; 

namespace StudentGradesApp
{
    class Program
    {
        static DbProviderFactory _factory;
        static string _connectionString;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Выберите СУБД:");
            Console.WriteLine("1. SQL Server");
            Console.WriteLine("2. MS Access (OleDb)");
            Console.Write("> ");
            string choice = Console.ReadLine();

            string providerName = choice == "2" ? "System.Data.OleDb" : "System.Data.SqlClient";
            string connStringName = choice == "2" ? "OleDbProvider" : "SqlProvider";

            try
            {
                _factory = DbProviderFactories.GetFactory(providerName);

                // Безопасно ищем строку подключения
                var connSetting = ConfigurationManager.ConnectionStrings[connStringName];
                if (connSetting == null)
                {
                    Console.WriteLine($"Ошибка: В App.config не найдена строка подключения с именем '{connStringName}'!");
                    Console.ReadLine();
                    return; // Выходим из программы
                }

                _connectionString = connSetting.ConnectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка инициализации: " + ex.Message);
                return;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==== Работа с БД «Оценки студентов» ====");
                Console.WriteLine("1. Асинхронное чтение студентов (Задание 3)");
                Console.WriteLine("2. Асинхронное добавление оценки (Задание 4)");
                Console.WriteLine("3. Асинхронное удаление оценки (Задание 5)");
                Console.WriteLine("0. Выход");
                Console.Write("\n> ");

                string action = Console.ReadLine();
                if (action == "0") break;

                Stopwatch sw = Stopwatch.StartNew();

                try
                {
                    switch (action)
                    {
                        case "1": await ReadStudentsAsync(); break;
                        case "2": await AddGradeAsync(); break;
                        case "3": await DeleteGradeAsync(); break;
                        default: Console.WriteLine("Нет такого пункта."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ОШИБКА: " + ex.Message);
                }

                sw.Stop();
                Console.WriteLine($"\n[Время выполнения: {sw.Elapsed.TotalSeconds:F3} сек.]");
                Console.WriteLine("Нажмите Enter...");
                Console.ReadLine();
            }
        }

        static async Task ReadStudentsAsync()
        {
            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                await conn.OpenAsync();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT s.FullName, s.GroupName, g.Subject, g.GradeValue " +
                                      "FROM Students s JOIN Grades g ON s.Id = g.StudentId";
                    using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        Console.WriteLine("\nСтудент | Группа | Предмет | Оценка");
                        Console.WriteLine(new string('-', 50));
                        while (await reader.ReadAsync())
                        {
                            Console.WriteLine($"{reader["FullName"],-15} | {reader["GroupName"],-10} | {reader["Subject"],-15} | {reader["GradeValue"]}");
                        }
                    }
                }
            }
        }

        static async Task AddGradeAsync()
        {
            Console.Write("Введите ID студента: ");
            int studentId = int.Parse(Console.ReadLine());
            Console.Write("Введите предмет: ");
            string subject = Console.ReadLine();
            Console.Write("Введите оценку: ");
            int grade = int.Parse(Console.ReadLine());

            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                await conn.OpenAsync();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    bool isSql = _factory is System.Data.SqlClient.SqlClientFactory;
                    cmd.CommandText = isSql
                        ? "INSERT INTO Grades (StudentId, Subject, GradeValue) VALUES (@sid, @subj, @val)"
                        : "INSERT INTO Grades (StudentId, Subject, GradeValue) VALUES (?, ?, ?)";

                    DbParameter p1 = cmd.CreateParameter(); p1.ParameterName = isSql ? "@sid" : "?"; p1.Value = studentId;
                    DbParameter p2 = cmd.CreateParameter(); p2.ParameterName = isSql ? "@subj" : "?"; p2.Value = subject;
                    DbParameter p3 = cmd.CreateParameter(); p3.ParameterName = isSql ? "@val" : "?"; p3.Value = grade;
                    cmd.Parameters.Add(p1); cmd.Parameters.Add(p2); cmd.Parameters.Add(p3);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"Добавлено строк: {rows}");
                }
            }
        }

        static async Task DeleteGradeAsync()
        {
            Console.Write("Введите ID оценки для удаления: ");
            int gradeId = int.Parse(Console.ReadLine());

            using (DbConnection conn = _factory.CreateConnection())
            {
                conn.ConnectionString = _connectionString;
                await conn.OpenAsync();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    bool isSql = _factory is System.Data.SqlClient.SqlClientFactory;
                    cmd.CommandText = isSql
                        ? "DELETE FROM Grades WHERE Id = @id"
                        : "DELETE FROM Grades WHERE Id = ?";

                    DbParameter p1 = cmd.CreateParameter(); p1.ParameterName = isSql ? "@id" : "?"; p1.Value = gradeId;
                    cmd.Parameters.Add(p1);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"Удалено строк: {rows}");
                }
            }
        }
    }
}