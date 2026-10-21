using System;
using System.Linq;
using System.Data.Entity;

namespace GameStore.ConsoleApp
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Studio { get; set; }
        public string Style { get; set; }
        public DateTime ReleaseDate { get; set; }

        public string GameMode { get; set; } 
        public int CopiesSold { get; set; }
    }

    // Контекст базы данных
    public class GameContext : DbContext
    {
        public GameContext() : base("Server=localhost;Database=GameStoreDb;Trusted_Connection=True;") { }
        public DbSet<Game> Games { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            using (var db = new GameContext())
            {
                if (!db.Games.Any())
                {
                    db.Games.Add(new Game { Name = "Ведьмак 3", Studio = "CD Projekt", Style = "RPG", ReleaseDate = new DateTime(2015, 5, 19), GameMode = "Однопользовательский", CopiesSold = 50000000 });
                    db.Games.Add(new Game { Name = "GTA V", Studio = "Rockstar", Style = "Action", ReleaseDate = new DateTime(2013, 9, 17), GameMode = "Многопользовательский", CopiesSold = 185000000 });
                    db.Games.Add(new Game { Name = "Minecraft", Studio = "Mojang", Style = "Sandbox", ReleaseDate = new DateTime(2011, 11, 18), GameMode = "Многопользовательский", CopiesSold = 300000000 });
                    db.Games.Add(new Game { Name = "Cyberpunk 2077", Studio = "CD Projekt", Style = "RPG", ReleaseDate = new DateTime(2020, 12, 10), GameMode = "Однопользовательский", CopiesSold = 25000000 });
                    db.SaveChanges();
                }

                bool exit = false;
                while (!exit)
                {
                    Console.Clear();
                    Console.WriteLine("=== БАЗА ИГР ===");
                    Console.WriteLine("1. Поиск по названию игры");
                    Console.WriteLine("2. Поиск по студии");
                    Console.WriteLine("3. Поиск по студии и названию");
                    Console.WriteLine("4. Поиск по стилю");
                    Console.WriteLine("5. Поиск по году релиза");
                    Console.WriteLine("6. Однопользовательские игры");
                    Console.WriteLine("7. Многопользовательские игры");
                    Console.WriteLine("8. Самая продаваемая игра");
                    Console.WriteLine("9. Самая непродаваемая игра (минимум продаж)");
                    Console.WriteLine("10. Топ-3 продаваемых игр");
                    Console.WriteLine("11. Топ-3 непродаваемых игр");
                    Console.WriteLine("12. Добавить игру");
                    Console.WriteLine("13. Изменить игру");
                    Console.WriteLine("14. Удалить игру");
                    Console.WriteLine("0. Выход");
                    Console.Write("Выберите действие: ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            Console.Write("Введите название игры: ");
                            string searchName = Console.ReadLine();
                            var gamesByName = db.Games.Where(g => g.Name.Contains(searchName)).ToList();
                            PrintGames(gamesByName);
                            break;
                        case "2":
                            Console.Write("Введите название студии: ");
                            string searchStudio = Console.ReadLine();
                            var gamesByStudio = db.Games.Where(g => g.Studio.Contains(searchStudio)).ToList();
                            PrintGames(gamesByStudio);
                            break;
                        case "3":
                            Console.Write("Введите название игры: ");
                            string searchN = Console.ReadLine();
                            Console.Write("Введите студию: ");
                            string searchS = Console.ReadLine();
                            var gamesByBoth = db.Games.Where(g => g.Name.Contains(searchN) && g.Studio.Contains(searchS)).ToList();
                            PrintGames(gamesByBoth);
                            break;
                        case "4":
                            Console.Write("Введите стиль игры: ");
                            string searchStyle = Console.ReadLine();
                            var gamesByStyle = db.Games.Where(g => g.Style.Contains(searchStyle)).ToList();
                            PrintGames(gamesByStyle);
                            break;
                        case "5":
                            Console.Write("Введите год релиза (например, 2015): ");
                            int searchYear = int.Parse(Console.ReadLine());
                            var gamesByYear = db.Games.Where(g => g.ReleaseDate.Year == searchYear).ToList();
                            PrintGames(gamesByYear);
                            break;

                        case "6":
                            var singlePlayer = db.Games.Where(g => g.GameMode == "Однопользовательский").ToList();
                            PrintGames(singlePlayer, "Однопользовательские игры");
                            break;
                        case "7":
                            var multiPlayer = db.Games.Where(g => g.GameMode == "Многопользовательский").ToList();
                            PrintGames(multiPlayer, "Многопользовательские игры");
                            break;
                        case "8":
                            var maxSold = db.Games.OrderByDescending(g => g.CopiesSold).FirstOrDefault();
                            PrintGames(new[] { maxSold }, "Самая продаваемая игра");
                            break;
                        case "9":
                            var minSold = db.Games.OrderBy(g => g.CopiesSold).FirstOrDefault();
                            PrintGames(new[] { minSold }, "Самая непродаваемая игра");
                            break;
                        case "10":
                            var top3Sold = db.Games.OrderByDescending(g => g.CopiesSold).Take(3).ToList();
                            PrintGames(top3Sold, "Топ-3 продаваемых");
                            break;
                        case "11":
                            var top3UnSold = db.Games.OrderBy(g => g.CopiesSold).Take(3).ToList();
                            PrintGames(top3UnSold, "Топ-3 непродаваемых");
                            break;

                        case "12":
                            Console.WriteLine("--- Добавление игры ---");
                            Console.Write("Название: "); string newName = Console.ReadLine();
                            Console.Write("Студия: "); string newStudio = Console.ReadLine();

                            if (db.Games.Any(g => g.Name.ToLower() == newName.ToLower() && g.Studio.ToLower() == newStudio.ToLower()))
                            {
                                Console.WriteLine("Ошибка: Такая игра от этой студии уже существует!");
                                break;
                            }

                            Console.Write("Стиль: "); string newStyle = Console.ReadLine();
                            Console.Write("Год релиза (только цифры): "); int newYear = int.Parse(Console.ReadLine());
                            Console.Write("Режим (Однопользовательский/Многопользовательский): "); string newMode = Console.ReadLine();
                            Console.Write("Продано копий: "); int newCopies = int.Parse(Console.ReadLine());

                            db.Games.Add(new Game
                            {
                                Name = newName,
                                Studio = newStudio,
                                Style = newStyle,
                                ReleaseDate = new DateTime(newYear, 1, 1),
                                GameMode = newMode,
                                CopiesSold = newCopies
                            });
                            db.SaveChanges();
                            Console.WriteLine("Игра успешно добавлена!");
                            break;

                        case "13":
                            Console.WriteLine("--- Изменение игры ---");
                            Console.Write("Введите название игры для изменения: ");
                            string editName = Console.ReadLine();
                            var gameToEdit = db.Games.FirstOrDefault(g => g.Name.ToLower() == editName.ToLower());

                            if (gameToEdit != null)
                            {
                                Console.WriteLine($"Текущие данные: {gameToEdit.Name} | {gameToEdit.Studio} | Продажи: {gameToEdit.CopiesSold}");
                                Console.WriteLine("Что изменить? (1-Название, 2-Студия, 3-Стиль, 4-Режим, 5-Продажи)");
                                string editChoice = Console.ReadLine();

                                switch (editChoice)
                                {
                                    case "1": Console.Write("Новое название: "); gameToEdit.Name = Console.ReadLine(); break;
                                    case "2": Console.Write("Новая студия: "); gameToEdit.Studio = Console.ReadLine(); break;
                                    case "3": Console.Write("Новый стиль: "); gameToEdit.Style = Console.ReadLine(); break;
                                    case "4": Console.Write("Новый режим: "); gameToEdit.GameMode = Console.ReadLine(); break;
                                    case "5": Console.Write("Новые продажи: "); gameToEdit.CopiesSold = int.Parse(Console.ReadLine()); break;
                                }
                                db.SaveChanges();
                                Console.WriteLine("Игра успешно изменена!");
                            }
                            else Console.WriteLine("Игра не найдена.");
                            break;

                        case "14":
                            Console.WriteLine("--- Удаление игры ---");
                            Console.Write("Название игры: "); string delName = Console.ReadLine();
                            Console.Write("Студия: "); string delStudio = Console.ReadLine();

                            var gameToDelete = db.Games.FirstOrDefault(g => g.Name.ToLower() == delName.ToLower() && g.Studio.ToLower() == delStudio.ToLower());

                            if (gameToDelete != null)
                            {
                                Console.Write($"Вы уверены, что хотите удалить '{gameToDelete.Name}'? (y/n): ");
                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    db.Games.Remove(gameToDelete);
                                    db.SaveChanges();
                                    Console.WriteLine("Игра удалена.");
                                }
                                else Console.WriteLine("Удаление отменено.");
                            }
                            else Console.WriteLine("Игра не найдена.");
                            break;

                        case "0":
                            exit = true;
                            break;
                    }

                    if (!exit)
                    {
                        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                    }
                }
            }
        }

        static void PrintGames(System.Collections.Generic.IEnumerable<Game> games, string title = "Результаты поиска")
        {
            Console.WriteLine($"\n--- {title} ---");
            if (!games.Any()) { Console.WriteLine("Ничего не найдено."); return; }

            foreach (var g in games)
            {
                Console.WriteLine($"{g.Name} | Студия: {g.Studio} | Стиль: {g.Style} | Год: {g.ReleaseDate.Year} | Режим: {g.GameMode} | Продажи: {g.CopiesSold}");
            }
        }
    }
}