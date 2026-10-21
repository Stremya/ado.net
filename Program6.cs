using System;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AdoNetModule3
{
    class Program
    {
        static string currentProviderName = "System.Data.SqlClient";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\n--- МЕНЮ (База: VegetablesAndFruits) ---");
                Console.WriteLine("1. Прочитать данные (Асинхронно + Замер времени)");
                Console.WriteLine("2. Добавить запись (Асинхронно)");
                Console.WriteLine("3. Обновить запись (Асинхронно)");
                Console.WriteLine("4. Удалить запись (Асинхронно)");
                Console.WriteLine("5. Сменить СУБД (Текущая: " + currentProviderName + ")");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");
                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": await ReadDataAsync(); break;
                        case "2": await InsertDataAsync(); break;
                        case "3": await UpdateDataAsync(); break;
                        case "4": await DeleteDataAsync(); break;
                        case "5": SwitchProvider(); break;
                        case "0": exit = true; break;
                        default: Console.WriteLine("Неверный выбор."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ОШИБКА: " + ex.Message);
                }
            }
        }

        static string GetConnectionString()
        {
            foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
            {
                if (cs.ProviderName == currentProviderName)
                    return cs.ConnectionString;
            }
            throw new Exception("Строка подключения для провайдера не найдена в App.config!");
        }

        static void SwitchProvider()
        {
            if (currentProviderName == "System.Data.SqlClient")
                currentProviderName = "System.Data.SqlClient"; 
            else
                currentProviderName = "System.Data.SqlClient";

            Console.WriteLine($"Текущая СУБД: {currentProviderName}. (Для SQLite нужен NuGet пакет)");
        }

        static async Task ReadDataAsync()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(currentProviderName);
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                await conn.OpenAsync();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Fruits";

                    Stopwatch sw = Stopwatch.StartNew();

                    using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader.GetName(i).PadRight(15) + " | ");
                        }
                        Console.WriteLine("\n" + new string('-', reader.FieldCount * 18));

                        while (await reader.ReadAsync())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string val = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();
                                Console.Write(val.PadRight(15) + " | ");
                            }
                            Console.WriteLine();
                        }
                    }
                    sw.Stop();
                    Console.WriteLine($"\nВремя выполнения запроса: {sw.Elapsed.TotalSeconds:F4} сек.");
                }
            }
        }

        static async Task InsertDataAsync()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(currentProviderName);
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                await conn.OpenAsync();

                DataTable schema = conn.GetSchema("Columns", new[] { null, null, "Fruits", null });
                List<string> insertColumns = new List<string>();

                Console.WriteLine("\nВведите данные для новой записи (оставьте пустым, если NULL):");
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach (DataRow row in schema.Rows)
                {
                    string colName = row["COLUMN_NAME"].ToString();

                    if (colName.Equals("Id", StringComparison.OrdinalIgnoreCase)) continue;

                    Console.Write($"Значение для '{colName}': ");
                    string input = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        insertColumns.Add(colName);
                        values.Add(colName, input);
                    }
                }

                if (insertColumns.Count == 0)
                {
                    Console.WriteLine("Нет данных для вставки.");
                    return;
                }

                string cols = string.Join(", ", insertColumns);
                string prms = "@" + string.Join(", @", insertColumns);

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"INSERT INTO Fruits ({cols}) VALUES ({prms})";

                    foreach (var kvp in values)
                    {
                        DbParameter p = cmd.CreateParameter();
                        p.ParameterName = "@" + kvp.Key;
                        p.Value = kvp.Value;
                        cmd.Parameters.Add(p);
                    }

                    await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine("Запись успешно добавлена!");
                }
            }
        }

        static async Task UpdateDataAsync()
        {
            Console.Write("Введите ID записи для обновления: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            Console.Write("Введите НОВОЕ название фрукта: ");
            string newName = Console.ReadLine();

            DbProviderFactory factory = DbProviderFactories.GetFactory(currentProviderName);
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                await conn.OpenAsync();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Fruits SET Name = @name WHERE Id = @id";

                    DbParameter pName = cmd.CreateParameter();
                    pName.ParameterName = "@name";
                    pName.Value = newName;
                    cmd.Parameters.Add(pName);

                    DbParameter pId = cmd.CreateParameter();
                    pId.ParameterName = "@id";
                    pId.Value = id;
                    cmd.Parameters.Add(pId);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine(rows > 0 ? "Запись обновлена!" : "Запись с таким ID не найдена.");
                }
            }
        }

        static async Task DeleteDataAsync()
        {
            Console.Write("Введите ID записи для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            DbProviderFactory factory = DbProviderFactories.GetFactory(currentProviderName);
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                await conn.OpenAsync();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Fruits WHERE Id = @id";

                    DbParameter pId = cmd.CreateParameter();
                    pId.ParameterName = "@id";
                    pId.Value = id;
                    cmd.Parameters.Add(pId);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine(rows > 0 ? "Запись удалена!" : "Запись с таким ID не найдена.");
                }
            }
        }
    }
}