using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace ADO_Lab3
{
    class Program
    {
        static string cs = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Library;Integrated Security=SSPI;";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==== Лабораторная №3 — ADO.NET ====");
                Console.WriteLine("1. Список поставщиков данных");
                Console.WriteLine("2. Запрос через DbProviderFactory");
                Console.WriteLine("3. DataViewManager — фильтр и сортировка");
                Console.WriteLine("4. Успешная транзакция");
                Console.WriteLine("5. Транзакция с откатом");
                Console.WriteLine("0. Выход");
                Console.Write("\n> ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1": ShowProviders(); break;
                        case "2": RunQueryViaFactory(); break;
                        case "3": DemoDataViewModel(); break;
                        case "4": SuccessfulTransaction(); break;
                        case "5": FailedTransaction(); break;
                        case "0": return;
                        default: Console.WriteLine("Нет такого пункта."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ОШИБКА: " + ex.Message);
                }

                Console.WriteLine("\nНажмите Enter...");
                Console.ReadLine();
            }
        }

        static void ShowProviders()
        {
            DataTable providers = DbProviderFactories.GetFactoryClasses();
            Console.WriteLine("=== Зарегистрированные поставщики ===\n");
            Console.WriteLine("{0,-25} | {1}", "InvariantName", "Description");
            Console.WriteLine(new string('-', 80));

            foreach (DataRow row in providers.Rows)
            {
                Console.WriteLine("{0,-25} | {1}",
                    row["InvariantName"], row["Description"]);
            }
            Console.WriteLine("\nВсего найдено: " + providers.Rows.Count);
        }

        static void RunQueryViaFactory()
        {
            Console.WriteLine("Выберите поставщика:");
            Console.WriteLine("  System.Data.SqlClient");
            Console.WriteLine("  System.Data.OleDb");
            Console.Write("> ");
            string providerName = Console.ReadLine().Trim();

            DbProviderFactory factory;
            try
            {
                factory = DbProviderFactories.GetFactory(providerName);
            }
            catch
            {
                Console.WriteLine("Поставщик не найден!");
                return;
            }

            string connStr = GetConnectionStringByProvider(providerName);
            if (string.IsNullOrEmpty(connStr))
            {
                Console.WriteLine("Строка подключения не найдена в App.config.");
                return;
            }

            Console.WriteLine("\nСтрока подключения: " + connStr);

            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = connStr;
                DbCommand cmd = factory.CreateCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT * FROM Books";

                DbDataAdapter adapter = factory.CreateDataAdapter();
                adapter.SelectCommand = cmd;

                DataTable dt = new DataTable();
                try
                {
                    conn.Open();
                    adapter.Fill(dt);
                    Console.WriteLine("\n=== Результат SELECT * FROM Books ===\n");
                    PrintDataTable(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }
            }
        }

        static string GetConnectionStringByProvider(string providerName)
        {
            foreach (ConnectionStringSettings s in ConfigurationManager.ConnectionStrings)
            {
                if (s.ProviderName == providerName)
                    return s.ConnectionString;
            }
            return null;
        }

        static void DemoDataViewModel()
        {
            DataSet set = new DataSet();
            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Authors", conn);
                adapter.Fill(set, "Authors");
            }

            Console.WriteLine("=== Исходные данные (без фильтра) ===\n");
            PrintDataTable(set.Tables["Authors"]);

            DataViewManager dvm = new DataViewManager(set);
            dvm.DataViewSettings["Authors"].RowFilter = "Id < 100";
            dvm.DataViewSettings["Authors"].Sort = "LastName ASC";

            DataView dv = dvm.CreateDataView(set.Tables["Authors"]);

            Console.WriteLine("\n=== После фильтра (Id < 100) и сортировки (LastName ASC) ===\n");
            PrintDataView(dv);

            Console.WriteLine("\n=== Исходные данные после фильтрации (не изменились!) ===\n");
            PrintDataTable(set.Tables["Authors"]);
        }

        static void SuccessfulTransaction()
        {
            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                new SqlCommand("IF OBJECT_ID('dbo.tmp3', 'U') IS NOT NULL DROP TABLE dbo.tmp3", c)
                    .ExecuteNonQuery();
            }

            SqlConnection conn = new SqlConnection(cs);
            SqlTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                SqlCommand comm = conn.CreateCommand();
                comm.Transaction = tran;

                comm.CommandText = "CREATE TABLE tmp3(id INT IDENTITY PRIMARY KEY, f1 VARCHAR(20), f2 INT)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Создана таблица tmp3.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('Hello', 100)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 1.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('World', 200)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 2.");

                tran.Commit();
                Console.WriteLine("\n✔ ТРАНЗАКЦИЯ ЗАФИКСИРОВАНА.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                if (tran != null)
                {
                    tran.Rollback();
                    Console.WriteLine("ТРАНЗАКЦИЯ ОТКАЧЕНА.");
                }
            }
            finally
            {
                conn.Close();
            }

            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                int count = (int)new SqlCommand("SELECT COUNT(*) FROM tmp3", c).ExecuteScalar();
                Console.WriteLine("\nПроверка: строк в tmp3 = " + count + " (ожидается 2).");
            }

            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                new SqlCommand("DROP TABLE tmp3", c).ExecuteNonQuery();
                Console.WriteLine("Таблица tmp3 удалена.");
            }
        }

        static void FailedTransaction()
        {
            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                new SqlCommand("IF OBJECT_ID('dbo.tmp3', 'U') IS NOT NULL DROP TABLE dbo.tmp3", c)
                    .ExecuteNonQuery();
            }

            SqlConnection conn = new SqlConnection(cs);
            SqlTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                SqlCommand comm = conn.CreateCommand();
                comm.Transaction = tran;

                comm.CommandText = "CREATE TABLE tmp3(id INT IDENTITY PRIMARY KEY, f1 VARCHAR(20), f2 INT)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Создана таблица tmp3.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('Hello', 100)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 1.");

                comm.CommandText = "INSERT INTO tmp4(f1, f2) VALUES('World', 200)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Эта строка НЕ должна выполниться.");

                tran.Commit(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n✘ ОШИБКА: " + ex.Message);
                if (tran != null)
                {
                    tran.Rollback();
                    Console.WriteLine("ТРАНЗАКЦИЯ ОТКАЧЕНА – все шаги отменены.");
                }
            }
            finally
            {
                conn.Close();
            }

            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                int count = (int)new SqlCommand(
                    "SELECT COUNT(*) FROM sys.tables WHERE name='tmp3'", c)
                    .ExecuteScalar();
                Console.WriteLine("\nПроверка: таблиц tmp3 в БД = " + count + " (ожидается 0).");
            }
        }

        static void PrintDataTable(DataTable dt)
        {
            if (dt.Rows.Count == 0) { Console.WriteLine("(нет данных)"); return; }

            foreach (DataColumn col in dt.Columns)
                Console.Write(col.ColumnName.PadRight(15) + "|");
            Console.WriteLine();
            Console.WriteLine(new string('-', 16 * dt.Columns.Count));

            foreach (DataRow row in dt.Rows)
            {
                foreach (var v in row.ItemArray)
                    Console.Write((v?.ToString() ?? "").PadRight(15) + "|");
                Console.WriteLine();
            }
        }

        static void PrintDataView(DataView dv)
        {
            foreach (DataColumn col in dv.Table.Columns)
                Console.Write(col.ColumnName.PadRight(15) + "|");
            Console.WriteLine();
            Console.WriteLine(new string('-', 16 * dv.Table.Columns.Count));

            foreach (DataRowView row in dv)
            {
                foreach (var v in row.Row.ItemArray)
                    Console.Write((v?.ToString() ?? "").PadRight(15) + "|");
                Console.WriteLine();
            }
        }
    }
}