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
    }

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
            Console.WriteLine("=== ДОМАШНЕЕ ЗАДАНИЕ: ИГРЫ ===");

            using (var db = new GameContext())
            {
                if (!db.Database.Exists())
                {
                    db.Database.Create();
                }

                if (!db.Games.Any())
                {
                    db.Games.Add(new Game { Name = "Ведьмак 3", Studio = "CD Projekt", Style = "RPG", ReleaseDate = new DateTime(2015, 5, 19) });
                    db.Games.Add(new Game { Name = "GTA V", Studio = "Rockstar", Style = "Action", ReleaseDate = new DateTime(2013, 9, 17) });
                    db.SaveChanges();
                }

                Console.WriteLine("\nСписок игр в базе:");
                foreach (var g in db.Games)
                {
                    Console.WriteLine($"- {g.Name} | Студия: {g.Studio} | Стиль: {g.Style} | Дата: {g.ReleaseDate.ToShortDateString()}");
                }
            }

            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }
    }
}