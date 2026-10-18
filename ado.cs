using System;
using System.Data;
using System.Data.SqlClient;

namespace WarehouseApp
{
    class Program
    {
        static private SqlConnection conn;

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(" 1. Подключиться к БД");
                Console.WriteLine(" 2. Отключиться от БД");
                Console.WriteLine(" 3. Все товары");
                Console.WriteLine(" 4. Все типы товаров");
                Console.WriteLine(" 5. Все поставщики");
                Console.WriteLine(" 6. Товар с макс. количеством");
                Console.WriteLine(" 7. Товар с мин. количеством");
                Console.WriteLine(" 8. Товар с мин. себестоимостью");
                Console.WriteLine(" 9. Товар с макс. себестоимостью");
                Console.WriteLine("10. Товары по категории");
                Console.WriteLine("11. Товары по поставщику");
                Console.WriteLine("12. Самый старый товар");
                Console.WriteLine("13. Среднее кол-во по типу");
                Console.WriteLine(" 0. Выход");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine();
                Console.Clear();

                try
                {
                    switch (choice)
                    {
                        case "1": Connect(); break;
                        case "2": Disconnect(); break;
                        case "3": CheckConn(); ShowAllProducts(); break;
                        case "4": CheckConn(); ShowAllTypes(); break;
                        case "5": CheckConn(); ShowAllSuppliers(); break;
                        case "6": CheckConn(); ShowProductByQty("MAX"); break;
                        case "7": CheckConn(); ShowProductByQty("MIN"); break;
                        case "8": CheckConn(); ShowProductByCost("MIN"); break;
                        case "9": CheckConn(); ShowProductByCost("MAX"); break;
                        case "10": CheckConn(); ShowByCategory(); break;
                        case "11": CheckConn(); ShowBySupplier(); break;
                        case "12": CheckConn(); ShowOldestProduct(); break;
                        case "13": CheckConn(); ShowAvgQtyByType(); break;
                        case "0": Disconnect(); return;
                        default: Console.WriteLine("Неверный выбор."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }

                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        static public void Connect()
        {
            try
            {
                conn = new SqlConnection();

                conn.Open();
                Console.WriteLine("Успешно подключено к базе данных «Склад»!");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
            }
        }

        static public void Disconnect()
        {
            if (conn != null && conn.State == ConnectionState.Open)
            {
                conn.Close();
                Console.WriteLine("🔌 Отключено от базы данных.");
            }
            else
            {
                Console.WriteLine("Подключение не было установлено.");
            }
        }

        static public void CheckConn()
        {
            if (conn == null || conn.State != ConnectionState.Open)
                throw new Exception("Сначала подключитесь к БД (пункт 1).");
        }

        static public void ShowAllProducts()
        {
            string sql = @"
                SELECT p.ProductID, p.ProductName, t.TypeName,
                       s.SupplierName, p.Quantity, p.CostPrice, p.DeliveryDate
                FROM Products p
                JOIN ProductTypes t ON p.TypeID = t.TypeID
                JOIN Suppliers s    ON p.SupplierID = s.SupplierID";
            PrintTable(sql, "ВСЕ ТОВАРЫ");
        }

        static public void ShowAllTypes()
        {
            PrintTable("SELECT TypeID, TypeName FROM ProductTypes", "ТИПЫ ТОВАРОВ");
        }

        static public void ShowAllSuppliers()
        {
            PrintTable("SELECT SupplierID, SupplierName FROM Suppliers", "ПОСТАВЩИКИ");
        }

        static public void ShowProductByQty(string mode)
        {
            string sql = $@"
                SELECT TOP 1 p.ProductName, p.Quantity
                FROM Products p
                ORDER BY p.Quantity {(mode == "MAX" ? "DESC" : "ASC")}";
            PrintTable(sql, mode == "MAX"
                ? "ТОВАР С МАКС. КОЛИЧЕСТВОМ"
                : "ТОВАР С МИН. КОЛИЧЕСТВОМ");
        }

        static public void ShowProductByCost(string mode)
        {
            string sql = $@"
                SELECT TOP 1 p.ProductName, p.CostPrice
                FROM Products p
                ORDER BY p.CostPrice {(mode == "MIN" ? "ASC" : "DESC")}";
            PrintTable(sql, mode == "MIN"
                ? "ТОВАР С МИН. СЕБЕСТОИМОСТЬЮ"
                : "ТОВАР С МАКС. СЕБЕСТОИМОСТЬЮ");
        }

        static public void ShowByCategory()
        {
            PrintTable("SELECT TypeID, TypeName FROM ProductTypes", "Доступные категории:");
            Console.Write("Введите название категории: ");
            string cat = Console.ReadLine();

            string sql = @"
                SELECT p.ProductName, p.Quantity, p.CostPrice
                FROM Products p
                JOIN ProductTypes t ON p.TypeID = t.TypeID
                WHERE t.TypeName = @cat";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@cat", cat);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n--- Товары категории «{cat}» ---");
                    if (!reader.HasRows) { Console.WriteLine("Не найдено."); return; }
                    while (reader.Read())
                        Console.WriteLine($"  {reader["ProductName"]}, " +
                            $"кол-во: {reader["Quantity"]}, цена: {reader["CostPrice"]}");
                }
            }
        }

        static public void ShowBySupplier()
        {
            PrintTable("SELECT SupplierID, SupplierName FROM Suppliers", "Доступные поставщики:");
            Console.Write("Введите название поставщика: ");
            string sup = Console.ReadLine();

            string sql = @"
                SELECT p.ProductName, p.Quantity, p.CostPrice
                FROM Products p
                JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE s.SupplierName = @sup";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@sup", sup);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n--- Товары поставщика «{sup}» ---");
                    if (!reader.HasRows) { Console.WriteLine("Не найдено."); return; }
                    while (reader.Read())
                        Console.WriteLine($"  {reader["ProductName"]}, " +
                            $"кол-во: {reader["Quantity"]}, цена: {reader["CostPrice"]}");
                }
            }
        }

        static public void ShowOldestProduct()
        {
            string sql = @"
                SELECT TOP 1 ProductName, DeliveryDate
                FROM Products ORDER BY DeliveryDate ASC";
            PrintTable(sql, "САМЫЙ СТАРЫЙ ТОВАР");
        }

        static public void ShowAvgQtyByType()
        {
            string sql = @"
                SELECT t.TypeName, AVG(p.Quantity) AS AvgQty
                FROM Products p
                JOIN ProductTypes t ON p.TypeID = t.TypeID
                GROUP BY t.TypeName";
            PrintTable(sql, "СРЕДНЕЕ КОЛИЧЕСТВО ПО ТИПУ");
        }

        // ===== УНИВЕРСАЛЬНЫЙ ВЫВОД ТАБЛИЦЫ =====
        static public void PrintTable(string sql, string title)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine($"\n--- {title} ---");
                if (!reader.HasRows) { Console.WriteLine("Нет данных."); return; }

                // Заголовки
                for (int i = 0; i < reader.FieldCount; i++)
                    Console.Write($"{reader.GetName(i),-20}");
                Console.WriteLine(new string('-', reader.FieldCount * 20));

                // Строки
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                        Console.Write($"{reader[i],-20}");
                    Console.WriteLine();
                }
            }
        }
    }
}