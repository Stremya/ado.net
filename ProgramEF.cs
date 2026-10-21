using System;
using System.Collections.Generic;
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
        public bool IsPromo { get; set; } // Является ли акционным
        public DateTime PromoEndDate { get; set; } // Когда заканчивается акция
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; } // "М" или "Ж"
        public int Age { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int SectionId { get; set; } // На какой раздел подписан
    }

    class Program
    {
        // --- ИМИТАЦИЯ БАЗЫ ДАННЫХ ---
        static List<Section> Sections = new List<Section>();
        static List<Product> Products = new List<Product>();
        static List<Customer> Customers = new List<Customer>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            InitData();

            Console.WriteLine("=== ЗАДАНИЕ 2: Популярность разделов ===");
            // Подсчитываем, сколько покупателей подписано на каждый раздел
            var sectionPopularity = Customers.GroupBy(c => c.SectionId)
                .Select(g => new { SectionId = g.Key, Count = g.Count() })
                .Join(Sections, p => p.SectionId, s => s.Id, (p, s) => new { SectionName = s.Name, Count = p.Count })
                .OrderByDescending(x => x.Count).ToList();

            Console.WriteLine("Топ-3 популярных:");
            foreach (var item in sectionPopularity.Take(3)) Console.WriteLine($"{item.SectionName} - {item.Count} чел.");

            Console.WriteLine($"Самый популярный: {sectionPopularity.First().SectionName}");

            Console.WriteLine("Топ-3 непопулярных:");
            foreach (var item in sectionPopularity.AsEnumerable().Reverse().Take(3)) Console.WriteLine($"{item.SectionName} - {item.Count} чел.");

            Console.WriteLine($"Самый непопулярный: {sectionPopularity.Last().SectionName}");


            Console.WriteLine("\n=== ЗАДАНИЕ 3: Акционные товары по разделам ===");
            var promoBySection = Products.Where(p => p.IsPromo)
                .GroupBy(p => p.SectionId)
                .Select(g => new { SectionId = g.Key, Count = g.Count() })
                .Join(Sections, p => p.SectionId, s => s.Id, (p, s) => new { SectionName = s.Name, Count = p.Count })
                .OrderByDescending(x => x.Count).ToList();

            Console.WriteLine("Топ-3 по акциям:");
            foreach (var item in promoBySection.Take(3)) Console.WriteLine($"{item.SectionName} - {item.Count} акций");
            Console.WriteLine($"Больше всего акций: {promoBySection.First().SectionName}");

            Console.WriteLine("Топ-3 с наименьшим кол-вом акций:");
            foreach (var item in promoBySection.AsEnumerable().Reverse().Take(3)) Console.WriteLine($"{item.SectionName} - {item.Count} акций");
            Console.WriteLine($"Меньше всего акций: {promoBySection.Last().SectionName}");


            Console.WriteLine("\n=== ЗАДАНИЕ 4: Сроки акций и средний возраст ===");
            var today = DateTime.Now;

            // 1. Акции, которым осталось 3 дня
            var expiringIn3Days = Products.Where(p => p.IsPromo && (p.PromoEndDate - today).Days == 3).ToList();
            Console.WriteLine($"Акции, которым осталось 3 дня: {string.Join(", ", expiringIn3Days.Select(p => p.Name))}");

            // 2. Просроченные акции
            var expiredPromos = Products.Where(p => p.IsPromo && p.PromoEndDate < today).ToList();
            Console.WriteLine($"Просроченные акции: {string.Join(", ", expiredPromos.Select(p => p.Name))}");

            // 3. Перенос просроченных в "Архив"
            var archive = new List<Product>();
            foreach (var p in expiredPromos)
            {
                archive.Add(p);
                Products.Remove(p); // Удаляем из основного списка
            }
            Console.WriteLine($"Перенесено в архив: {archive.Count} товаров.");

            // 4. Средний возраст по разделам, городам, странам
            Console.WriteLine("\nСредний возраст по разделам:");
            var avgAgeBySection = Customers.GroupBy(c => c.SectionId)
                .Join(Sections, g => g.Key, s => s.Id, (g, s) => new { Раздел = s.Name, Возраст = g.Average(c => c.Age) });
            foreach (var item in avgAgeBySection) Console.WriteLine($"{item.Раздел}: {item.Возраст:F1} лет");

            Console.WriteLine("\nСредний возраст по городам:");
            var avgAgeByCity = Customers.GroupBy(c => c.City).Select(g => new { Город = g.Key, Возраст = g.Average(c => c.Age) });
            foreach (var item in avgAgeByCity) Console.WriteLine($"{item.Город}: {item.Возраст:F1} лет");

            Console.WriteLine("\nСредний возраст по странам:");
            var avgAgeByCountry = Customers.GroupBy(c => c.Country).Select(g => new { Страна = g.Key, Возраст = g.Average(c => c.Age) });
            foreach (var item in avgAgeByCountry) Console.WriteLine($"{item.Страна}: {item.Возраст:F1} лет");


            Console.WriteLine("\n=== ЗАДАНИЕ 5: Анализ по полу ===");

            // 1. Самый популярный раздел для каждого пола
            Console.WriteLine("Самый популярный раздел по полу:");
            var topSectionByGender = Customers.GroupBy(c => new { c.Gender, c.SectionId })
                .Select(g => new { Gender = g.Key.Gender, SectionId = g.Key.SectionId, Count = g.Count() })
                .GroupBy(x => x.Gender)
                .Select(g => g.OrderByDescending(x => x.Count).First())
                .Join(Sections, x => x.SectionId, s => s.Id, (x, s) => new { Пол = x.Gender, Раздел = s.Name });

            foreach (var item in topSectionByGender) Console.WriteLine($"{item.Пол}: {item.Раздел}");

            // 2. Топ-3 разделов для каждого пола
            Console.WriteLine("\nТоп-3 разделов для каждого пола:");
            var top3ByGender = Customers.GroupBy(c => new { c.Gender, c.SectionId })
                .Select(g => new { Gender = g.Key.Gender, SectionId = g.Key.SectionId, Count = g.Count() })
                .GroupBy(x => x.Gender);
            foreach (var group in top3ByGender)
            {
                Console.WriteLine($"--- {group.Key} ---");
                var top3 = group.OrderByDescending(x => x.Count).Take(3)
                    .Join(Sections, x => x.SectionId, s => s.Id, (x, s) => $"{s.Name} ({x.Count} чел.)");
                foreach (var item in top3) Console.WriteLine(item);
            }

            // 3. Количество покупателей каждого пола
            Console.WriteLine("\nКоличество покупателей по полу:");
            var countByGender = Customers.GroupBy(c => c.Gender).Select(g => new { Пол = g.Key, Кол - во = g.Count() });
            foreach (var item in countByGender) Console.WriteLine($"{item.Пол}: {item.Кол - во}");

            // 4. Количество покупателей каждого пола для каждой страны
            Console.WriteLine("\nКоличество покупателей по полу и странам:");
            var countByGenderAndCountry = Customers.GroupBy(c => new { c.Country, c.Gender })
                .Select(g => new { Страна = g.Key.Country, Пол = g.Key.Gender, Кол - во = g.Count() });
            foreach (var item in countByGenderAndCountry) Console.WriteLine($"{item.Страна} | {item.Пол}: {item.Кол - во}");

            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }

        // Заполнение тестовыми данными
        static void InitData()
        {
            Sections.Add(new Section { Id = 1, Name = "Спорт" });
            Sections.Add(new Section { Id = 2, Name = "Технологии" });
            Sections.Add(new Section { Id = 3, Name = "Кулинария" });
            Sections.Add(new Section { Id = 4, Name = "Мода" });

            Products.Add(new Product { Id = 1, Name = "Спортивные кроссовки", SectionId = 1, IsPromo = true, PromoEndDate = DateTime.Now.AddDays(3) });
            Products.Add(new Product { Id = 2, Name = "Умные часы", SectionId = 2, IsPromo = true, PromoEndDate = DateTime.Now.AddDays(-1) }); // Просрочена
            Products.Add(new Product { Id = 3, Name = "Кулинарная книга", SectionId = 3, IsPromo = false });
            Products.Add(new Product { Id = 4, Name = "Куртка", SectionId = 4, IsPromo = true, PromoEndDate = DateTime.Now.AddDays(10) });
            Products.Add(new Product { Id = 5, Name = "Гантели", SectionId = 1, IsPromo = true, PromoEndDate = DateTime.Now.AddDays(-5) }); // Просрочена
            Products.Add(new Product { Id = 6, Name = "Планшет", SectionId = 2, IsPromo = false });

            Customers.Add(new Customer { Id = 1, Name = "Иван", Gender = "М", Age = 25, Country = "Россия", City = "Москва", SectionId = 2 });
            Customers.Add(new Customer { Id = 2, Name = "Анна", Gender = "Ж", Age = 30, Country = "Россия", City = "СПб", SectionId = 3 });
            Customers.Add(new Customer { Id = 3, Name = "Петр", Gender = "М", Age = 40, Country = "Беларусь", City = "Минск", SectionId = 1 });
            Customers.Add(new Customer { Id = 4, Name = "Мария", Gender = "Ж", Age = 22, Country = "Россия", City = "Москва", SectionId = 4 });
            Customers.Add(new Customer { Id = 5, Name = "Олег", Gender = "М", Age = 35, Country = "Казахстан", City = "Алматы", SectionId = 2 });
            Customers.Add(new Customer { Id = 6, Name = "Елена", Gender = "Ж", Age = 28, Country = "Россия", City = "СПб", SectionId = 2 });
        }
    }
}