using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MailingListApp
{
    // --- МОДЕЛИ ДАННЫХ ---
    public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SectionId { get; set; }
        public bool IsPromo { get; set; }
        public DateTime PromoEndDate { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int SectionId { get; set; }
    }

    public class ArchiveProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime PromoEndDate { get; set; }
    }

    // --- КОНТЕКСТ БАЗЫ ДАННЫХ (ТОТ САМЫЙ ENTITY FRAMEWORK) ---
    public class AppDbContext : DbContext
    {
        public DbSet<Section> Sections { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ArchiveProduct> ArchiveProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Используем SQLite (база создастся сама в файле mailing.db)
            optionsBuilder.UseSqlite("Data Source=mailing.db");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            using (var db = new AppDbContext())
            {
                // Создаем базу и заполняем её, если она пустая
                db.Database.EnsureCreated();
                if (!db.Sections.Any()) InitData(db);

                Console.WriteLine("=== ЗАДАНИЕ 2: Популярность разделов ===");
                var sectionPopularity = db.Customers
                    .GroupBy(c => c.SectionId)
                    .Select(g => new { SectionId = g.Key, Count = g.Count() })
                    .Join(db.Sections, p => p.SectionId, s => s.Id, (p, s) => new { SectionName = s.Name, Count = p.Count })
                    .OrderByDescending(x => x.Count).ToList();

                Console.WriteLine("Топ-3 популярных:");
                foreach (var item in sectionPopularity.Take(3)) Console.WriteLine($"{item.SectionName} - {item.Count} чел.");
                Console.WriteLine($"Самый популярный: {sectionPopularity.First().SectionName}");

                Console.WriteLine("Топ-3 непопулярных:");
                foreach (var item in sectionPopularity.AsEnumerable().Reverse().Take(3)) Console.WriteLine($"{item.SectionName} - {item.Count} чел.");
                Console.WriteLine($"Самый непопулярный: {sectionPopularity.Last().SectionName}");


                Console.WriteLine("\n=== ЗАДАНИЕ 3: Акционные товары по разделам ===");
                var promoBySection = db.Products
                    .Where(p => p.IsPromo)
                    .GroupBy(p => p.SectionId)
                    .Select(g => new { SectionId = g.Key, Count = g.Count() })
                    .Join(db.Sections, p => p.SectionId, s => s.Id, (p, s) => new { SectionName = s.Name, Count = p.Count })
                    .OrderByDescending(x => x.Count).ToList();

                Console.WriteLine("Топ-3 по акциям:");
                foreach (var item in promoBySection.Take(3)) Console.WriteLine($"{item.SectionName} - {item.Count} акций");
                Console.WriteLine($"Больше всего акций: {promoBySection.First().SectionName}");

                Console.WriteLine("Топ-3 с наименьшим кол-вом акций:");
                foreach (var item in promoBySection.AsEnumerable().Reverse().Take(3)) Console.WriteLine($"{item.SectionName} - {item.Count} акций");
                Console.WriteLine($"Меньше всего акций: {promoBySection.Last().SectionName}");


                Console.WriteLine("\n=== ЗАДАНИЕ 4: Сроки акций и средний возраст ===");
                var today = DateTime.Now;

                var expiringIn3Days = db.Products.Where(p => p.IsPromo && EF.Functions.DateDiffDay(today, p.PromoEndDate) == 3).ToList();
                Console.WriteLine($"Акции, которым осталось 3 дня: {string.Join(", ", expiringIn3Days.Select(p => p.Name))}");

                var expiredPromos = db.Products.Where(p => p.IsPromo && p.PromoEndDate < today).ToList();
                Console.WriteLine($"Просроченные акции: {string.Join(", ", expiredPromos.Select(p => p.Name))}");

                // Перенос в архив
                foreach (var p in expiredPromos)
                {
                    db.ArchiveProducts.Add(new ArchiveProduct { Name = p.Name, PromoEndDate = p.PromoEndDate });
                    db.Products.Remove(p);
                }
                db.SaveChanges();
                Console.WriteLine($"Перенесено в архив: {expiredPromos.Count} товаров.");

                Console.WriteLine("\nСредний возраст по разделам:");
                var avgAgeBySection = db.Customers.GroupBy(c => c.SectionId)
                    .Select(g => new { SectionId = g.Key, AvgAge = g.Average(c => c.Age) })
                    .Join(db.Sections, g => g.SectionId, s => s.Id, (g, s) => new { Раздел = s.Name, Возраст = g.AvgAge });
                foreach (var item in avgAgeBySection) Console.WriteLine($"{item.Раздел}: {item.Возраст:F1} лет");

                Console.WriteLine("\nСредний возраст по городам:");
                var avgAgeByCity = db.Customers.GroupBy(c => c.City).Select(g => new { Город = g.Key, Возраст = g.Average(c => c.Age) });
                foreach (var item in avgAgeByCity) Console.WriteLine($"{item.Город}: {item.Возраст:F1} лет");

                Console.WriteLine("\nСредний возраст по странам:");
                var avgAgeByCountry = db.Customers.GroupBy(c => c.Country).Select(g => new { Страна = g.Key, Возраст = g.Average(c => c.Age) });
                foreach (var item in avgAgeByCountry) Console.WriteLine($"{item.Страна}: {item.Возраст:F1} лет");


                Console.WriteLine("\n=== ЗАДАНИЕ 5: Анализ по полу ===");
                Console.WriteLine("Самый популярный раздел по полу:");
                var topSectionByGender = db.Customers.GroupBy(c => new { c.Gender, c.SectionId })
                    .Select(g => new { Gender = g.Key.Gender, SectionId = g.Key.SectionId, Count = g.Count() })
                    .GroupBy(x => x.Gender)
                    .Select(g => g.OrderByDescending(x => x.Count).First())
                    .Join(db.Sections, x => x.SectionId, s => s.Id, (x, s) => new { Пол = x.Gender, Раздел = s.Name });
                foreach (var item in topSectionByGender) Console.WriteLine($"{item.Пол}: {item.Раздел}");

                Console.WriteLine("\nТоп-3 разделов для каждого пола:");
                var top3ByGender = db.Customers.GroupBy(c => new { c.Gender, c.SectionId })
                    .Select(g => new { Gender = g.Key.Gender, SectionId = g.Key.SectionId, Count = g.Count() })
                    .GroupBy(x => x.Gender);
                foreach (var group in top3ByGender)
                {
                    Console.WriteLine($"--- {group.Key} ---");
                    var top3 = group.OrderByDescending(x => x.Count).Take(3)
                        .Join(db.Sections, x => x.SectionId, s => s.Id, (x, s) => $"{s.Name} ({x.Count} чел.)");
                    foreach (var item in top3) Console.WriteLine(item);
                }

                Console.WriteLine("\nКоличество покупателей по полу:");
                var countByGender = db.Customers.GroupBy(c => c.Gender).Select(g => new { Пол = g.Key, Кол - во = g.Count() });
                foreach (var item in countByGender) Console.WriteLine($"{item.Пол}: {item.Кол - во}");

                Console.WriteLine("\nКоличество покупателей по полу и странам:");
                var countByGenderAndCountry = db.Customers.GroupBy(c => new { c.Country, c.Gender })
                    .Select(g => new { Страна = g.Key.Country, Пол = g.Key.Gender, Кол - во = g.Count() });
                foreach (var item in countByGenderAndCountry) Console.WriteLine($"{item.Страна} | {item.Пол}: {item.Кол - во}");
            }

            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }

        static void InitData(AppDbContext db)
        {
            var s1 = new Section { Name = "Спорт" }; var s2 = new Section { Name = "Технологии" };
            var s3 = new Section { Name = "Кулинария" }; var s4 = new Section { Name = "Мода" };
            db.Sections.AddRange(s1, s2, s3, s4);
            db.SaveChanges();

            db.Products.AddRange(
                new Product { Name = "Кроссовки", SectionId = s1.Id, IsPromo = true, PromoEndDate = DateTime.Now.AddDays(3) },
                new Product { Name = "Часы", SectionId = s2.Id, IsPromo = true, PromoEndDate = DateTime.Now.AddDays(-1) },
                new Product { Name = "Книга", SectionId = s3.Id, IsPromo = false },
                new Product { Name = "Куртка", SectionId = s4.Id, IsPromo = true, PromoEndDate = DateTime.Now.AddDays(10) },
                new Product { Name = "Гантели", SectionId = s1.Id, IsPromo = true, PromoEndDate = DateTime.Now.AddDays(-5) }
            );
            db.SaveChanges();

            db.Customers.AddRange(
                new Customer { Name = "Иван", Gender = "М", Age = 25, Country = "РФ", City = "Москва", SectionId = s2.Id },
                new Customer { Name = "Анна", Gender = "Ж", Age = 30, Country = "РФ", City = "СПб", SectionId = s3.Id },
                new Customer { Name = "Петр", Gender = "М", Age = 40, Country = "РБ", City = "Минск", SectionId = s1.Id },
                new Customer { Name = "Мария", Gender = "Ж", Age = 22, Country = "РФ", City = "Москва", SectionId = s4.Id },
                new Customer { Name = "Олег", Gender = "М", Age = 35, Country = "РК", City = "Алматы", SectionId = s2.Id },
                new Customer { Name = "Елена", Gender = "Ж", Age = 28, Country = "РФ", City = "СПб", SectionId = s2.Id }
            );
            db.SaveChanges();
        }
    }
}