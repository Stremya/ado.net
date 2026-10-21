using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace AdoNetLab2
{
    class Program
    {
        static string ConnString = ConfigurationManager
            .ConnectionStrings["MyConnString"].ConnectionString;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Task1_ManualDataTable();
            Task2_FillDataSet();
            Task3_AutoUpdate();
            Task4_CustomUpdate();
            Task5_RowStateOrder();
            Task6_TableMappings();
            Task7_PictureFromDb();

            Console.WriteLine("\nГотово. Нажмите любую клавишу...");
            Console.ReadKey();
        }

        static void PrintTable(DataTable table, string title = "")
        {
            if (!string.IsNullOrEmpty(title))
                Console.WriteLine($"\n== {title} ==");

            foreach (DataColumn col in table.Columns)
                Console.Write(col.ColumnName.PadRight(15));
            Console.WriteLine();
            Console.WriteLine(new string('-', table.Columns.Count * 15));

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    object val = row[col];
                    string s = val == DBNull.Value ? "NULL" : val.ToString();
                    Console.Write(s.PadRight(15));
                }
                Console.WriteLine();
            }
        }

        static void Task1_ManualDataTable()
        {
            DataTable table = new DataTable("Authors");
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("LastName", typeof(string));

            object[][] data =
            {
                new object[] { 1, "Isaac",    "Asimov" },
                new object[] { 2, "Ray",      "Bradbury" },
                new object[] { 3, "Clifford", "Simak" },
                new object[] { 4, "Roger",    "Zelazny" }
            };

            foreach (var d in data)
            {
                DataRow row = table.NewRow();
                row["Id"] = d[0];
                row["FirstName"] = d[1];
                row["LastName"] = d[2];
                table.Rows.Add(row);
            }

            // Демонстрация чтения
            Console.WriteLine("\n[Task1] Чтение через row[0]: " + table.Rows[0][0]);
            Console.WriteLine("[Task1] Чтение через row[\"FirstName\"]: " +
                              table.Rows[1]["FirstName"]);

            PrintTable(table, "Authors (manual)");
        }

        static void Task2_FillDataSet()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Authors", conn);
                DataSet ds = new DataSet();
                da.Fill(ds, "Authors");

                Console.WriteLine("\n[Task2] Таблиц в DataSet: " + ds.Tables.Count);
                PrintTable(ds.Tables["Authors"], "Authors (из БД)");

                // Демонстрация повторного Fill с тем же именем
                da.Fill(ds, "Authors");
                Console.WriteLine("[Task2] После повторного Fill строк: " +
                                  ds.Tables["Authors"].Rows.Count +
                                  " (данные НЕ дублируются, таблица перезаполняется)");
            }
        }

        static void Task3_AutoUpdate()
        {
            Console.WriteLine("\n===== Task3: SqlCommandBuilder =====");

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);
                SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(da);

                DataSet ds = new DataSet();
                da.Fill(ds, "Books");
                DataTable books = ds.Tables["Books"];

                Console.WriteLine("\nДо изменения:");
                PrintTable(books, "Books (before)");

                // Локальное изменение
                books.Rows[0]["Price"] = 999;

                Console.WriteLine("\nПосле локального изменения (в памяти):");
                PrintTable(books, "Books (local change)");

                // Синхронизация
                int rows = da.Update(ds, "Books");
                Console.WriteLine($"\n[Task3] Обновлено строк в БД: {rows}");

                // Генерация команд
                Console.WriteLine("INSERT: " + cmdBuilder.GetInsertCommand().CommandText);
                Console.WriteLine("UPDATE: " + cmdBuilder.GetUpdateCommand().CommandText);
                Console.WriteLine("DELETE: " + cmdBuilder.GetDeleteCommand().CommandText);

                // Перечитать из БД
                DataSet ds2 = new DataSet();
                da.Fill(ds2, "Books");
                PrintTable(ds2.Tables["Books"], "Books (re-read from DB)");
            }

            Console.WriteLine("\n--- Проблемный эксперимент (без PK) ---");
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT Title, Price, Pages FROM Books", conn);
                SqlCommandBuilder cb = new SqlCommandBuilder(da);
                DataSet ds = new DataSet();
                da.Fill(ds, "Books");

                ds.Tables["Books"].Rows[0]["Price"] = 111;
                try
                {
                    da.Update(ds, "Books");
                    Console.WriteLine("Update прошёл (неожиданно!)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ОШИБКА: " + ex.GetType().Name);
                    Console.WriteLine("Сообщение: " + ex.Message);
                }
            }
        }

        static void Task4_CustomUpdate()
        {
            Console.WriteLine("\n===== Task4: Custom UpdateCommand =====");

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);

                SqlCommand updateCmd = new SqlCommand(
                    "UPDATE Books SET Price = @pPrice WHERE Id = @pId", conn);

                SqlParameter pPrice = updateCmd.Parameters.Add("@pPrice", SqlDbType.Int);
                pPrice.SourceVersion = DataRowVersion.Current;
                pPrice.SourceColumn = "Price";

                SqlParameter pId = updateCmd.Parameters.Add("@pId", SqlDbType.Int);
                pId.SourceVersion = DataRowVersion.Original;
                pId.SourceColumn = "Id";

                da.UpdateCommand = updateCmd;

                DataSet ds = new DataSet();
                da.Fill(ds, "Books");
                DataTable books = ds.Tables["Books"];

                PrintTable(books, "Books (до)");

                books.Rows[0]["Price"] = 888;
                books.Rows[1]["Price"] = 444;

                int rows = da.Update(ds, "Books");
                Console.WriteLine($"[Task4] Обновлено: {rows}");

                DataSet ds2 = new DataSet();
                da.Fill(ds2, "Books");
                PrintTable(ds2.Tables["Books"], "Books (после)");
            }
        }

        static void Task5_RowStateOrder()
        {
            Console.WriteLine("\n===== Task5: Порядок синхронизации =====");

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);
                SqlCommandBuilder cb = new SqlCommandBuilder(da);

                DataSet ds = new DataSet();
                da.Fill(ds, "Books");
                DataTable books = ds.Tables["Books"];

                if (books.Rows.Count < 2)
                {
                    Console.WriteLine("Недостаточно строк для демонстрации.");
                    return;
                }

                int delId = (int)books.Rows[0]["Id"];
                books.Rows[0].Delete();
                books.Rows[1]["Price"] = 777;

                DataRow newRow = books.NewRow();
                newRow["AuthorId"] = 1;
                newRow["Title"] = "New Book";
                newRow["Price"] = 500;
                newRow["Pages"] = 100;
                books.Rows.Add(newRow);

                Console.WriteLine("\nСостояния строк:");
                foreach (DataRow row in books.Rows)
                {
                    string title = row.RowState == DataRowState.Deleted
                        ? "<deleted id=" + delId + ">"
                        : row["Title"].ToString();
                    Console.WriteLine($"  {title,20} -> {row.RowState}");
                }

                da.Update(books.Select(null, null, DataViewRowState.Deleted));
                da.Update(books.Select(null, null, DataViewRowState.ModifiedCurrent));
                da.Update(books.Select(null, null, DataViewRowState.Added));

                Console.WriteLine("[Task5] Все три изменения применены.");

                DataSet ds2 = new DataSet();
                da.Fill(ds2, "Books");
                PrintTable(ds2.Tables["Books"], "Books (после Update)");
            }
        }

        static void Task6_TableMappings()
        {
            Console.WriteLine("\n===== Task6: TableMappings =====");

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                SqlDataAdapter da1 = new SqlDataAdapter(
                    "SELECT * FROM Authors; SELECT * FROM Books;", conn);
                DataSet ds1 = new DataSet();
                da1.Fill(ds1);
                Console.WriteLine("Имена таблиц по умолчанию: " +
                    string.Join(", ", GetTableNames(ds1)));

                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT * FROM Authors; SELECT * FROM Books;", conn);
                da.TableMappings.Add("Table", "Authors");
                da.TableMappings.Add("Table1", "Books");

                DataSet ds = new DataSet();
                da.Fill(ds);

                Console.WriteLine("После TableMappings: " +
                    string.Join(", ", GetTableNames(ds)));

                PrintTable(ds.Tables["Authors"], "Authors (mapped)");
                PrintTable(ds.Tables["Books"], "Books (mapped)");

                SqlDataAdapter da2 = new SqlDataAdapter(
                    "SELECT * FROM Authors", conn);
                DataTableMapping dtm =
                    da2.TableMappings.Add("Table", "Authors");
                dtm.ColumnMappings.Add("Id", "Num");
                dtm.ColumnMappings.Add("FirstName", "Name");
                dtm.ColumnMappings.Add("LastName", "Surname");

                DataSet ds2 = new DataSet();
                da2.Fill(ds2);
                PrintTable(ds2.Tables["Authors"], "Authors (column-mapped)");
            }
        }

        static System.Collections.Generic.IEnumerable<string> GetTableNames(DataSet ds)
        {
            foreach (DataTable t in ds.Tables) yield return t.TableName;
        }

        static void Task7_PictureFromDb()
        {
            Console.WriteLine("\n===== Task7: Picture =====");
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT Picture FROM Pictures WHERE Id = @id", conn);
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = 1;

                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                {
                    Console.WriteLine("Картинка с Id=1 не найдена.");
                    return;
                }

                byte[] bytes = (byte[])ds.Tables[0].Rows[0]["Picture"];
                System.IO.File.WriteAllBytes("out.jpg", bytes);
                Console.WriteLine("Сохранено в out.jpg (" + bytes.Length + " байт)");
            }
        }
    }
}