<Query Kind="Program" />

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CountriesApp
{
    public class City
    {
        public string Name { get; set; }
        public long Population { get; set; }
    }

    public class Country
    {
        public string Name { get; set; }
        public string Capital { get; set; }
        public long Population { get; set; }
        public double Area { get; set; }
        public string PartOfWorld { get; set; }
        public List<City> Cities { get; set; } = new List<City>();
    }

    class Program
    {
        static List<Country> dbCountries = new List<Country>();
        static bool isConnected = false; 

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            InitData(); 

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== СИСТЕМА СТРАН ===");
                Console.WriteLine($"Статус подключения: {(isConnected ? "ПОДКЛЮЧЕНО" : "ОТКЛЮЧЕНО")}");
                Console.WriteLine("1. Подключиться к БД");
                Console.WriteLine("2. Отключиться от БД");
                Console.WriteLine("3. Сформировать отчет (Задания 2, 3, 4)");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        if (isConnected) Console.WriteLine("Уже подключено!");
                        else { isConnected = true; Console.WriteLine("Успешное подключение к базе данных!"); }
                        break;
                    case "2":
                        if (!isConnected) Console.WriteLine("Вы уже отключены.");
                        else { isConnected = false; Console.WriteLine("Вы отключились от базы данных."); }
                        break;
                    case "3":
                        if (!isConnected) { Console.WriteLine("Ошибка! Сначала подключитесь к БД."); }
                        else { ReportsMenu(); }
                        break;
                    case "0":
                        exit = true;
                        break;
                }
                if (!exit) { Console.WriteLine("\nНажмите любую клавишу..."); Console.ReadKey(); }
            }
        }

        // Меню отчетов (Задания 2, 3, 4)
        static void ReportsMenu()
        {
            Console.Clear();
            Console.WriteLine("=== СИСТЕМА ОТЧЕТОВ ===");
            Console.WriteLine("1. Полная информация о странах");
            Console.WriteLine("2. Частичная информация (выбор колонок)");
            Console.WriteLine("3. Информация о конкретной стране");
            Console.WriteLine("4. Города конкретной страны");
            Console.WriteLine("5. Страны на указанную букву");
            Console.WriteLine("6. Столицы на указанную букву");
            Console.WriteLine("7. Топ-3 столиц с НАИМЕНЬШИМ населением");
            Console.WriteLine("8. Топ-3 стран с НАИМЕНЬШИМ населением");
            Console.WriteLine("9. Среднее население столиц по частям света");
            Console.WriteLine("10. Топ-3 стран с НАИМЕНЬШИМ населением по каждой части света");
            Console.WriteLine("11. Топ-3 стран с НАИБОЛЬШИМ населением по каждой части света");
            Console.WriteLine("12. Среднее население городов в конкретной стране");
            Console.WriteLine("13. Город с наименьшим населением в конкретной стране");
            Console.WriteLine("0. Назад");
            Console.Write("\nВыберите отчет: ");

            string choice = Console.ReadLine();
            Console.WriteLine("\nКуда вывести результат? (1 - На экран, 2 - В файл report.txt)");
            string outputChoice = Console.ReadLine();
            bool toFile = outputChoice == "2";

            switch (choice)
            {
                case "1": 
                    WriteReport(dbCountries.Select(c => $"{c.Name} | Столица: {c.Capital} | Население: {c.Population} | Площадь: {c.Area} | Часть света: {c.PartOfWorld}"), toFile); 
                    break;
                case "2":
                    Console.Write("Сколько первых стран вывести? ");
                    int count = int.Parse(Console.ReadLine());
                    WriteReport(dbCountries.Take(count).Select(c => $"{c.Name} | Столица: {c.Capital}"), toFile);
                    break;
                case "3":
                    Console.Write("Введите название страны: ");
                    string cName = Console.ReadLine().ToLower();
                    WriteReport(dbCountries.Where(c => c.Name.ToLower() == cName).Select(c => $"{c.Name} | Столица: {c.Capital} | Население: {c.Population} | Площадь: {c.Area}"), toFile);
                    break;
                case "4":
                    Console.Write("Введите название страны: ");
                    string cName2 = Console.ReadLine().ToLower();
                    var country = dbCountries.FirstOrDefault(c => c.Name.ToLower() == cName2);
                    if (country != null) WriteReport(country.Cities.Select(city => $"Город: {city.Name} | Население: {city.Population}"), toFile);
                    else WriteReport(new[] { "Страна не найдена" }, toFile);
                    break;
                case "5":
                    Console.Write("Введите букву: ");
                    string letter = Console.ReadLine().ToLower();
                    WriteReport(dbCountries.Where(c => c.Name.ToLower().StartsWith(letter)).Select(c => c.Name), toFile);
                    break;
                case "6":
                    Console.Write("Введите букву: ");
                    string letter2 = Console.ReadLine().ToLower();
                    WriteReport(dbCountries.Where(c => c.Capital.ToLower().StartsWith(letter2)).Select(c => c.Capital), toFile);
                    break;
                case "7":
                    var top3MinCap = dbCountries.OrderBy(c => c.Cities.FirstOrDefault(city => city.Name == c.Capital)?.Population ?? 0).Take(3);
                    WriteReport(top3MinCap.Select(c => $"{c.Capital} ({c.Name}) - {c.Cities.FirstOrDefault(city => city.Name == c.Capital)?.Population ?? 0} чел."), toFile);
                    break;
                case "8":
                    var top3MinCountry = dbCountries.OrderBy(c => c.Population).Take(3);
                    WriteReport(top3MinCountry.Select(c => $"{c.Name} - {c.Population} чел."), toFile);
                    break;
                case "9":
                    var avgCapByPart = dbCountries.GroupBy(c => c.PartOfWorld)
                        .Select(g => $"{g.Key}: {(long)g.Average(c => c.Cities.FirstOrDefault(city => city.Name == c.Capital)?.Population ?? 0)} чел.");
                    WriteReport(avgCapByPart, toFile);
                    break;
                case "10":
                    var top3MinByPart = dbCountries.GroupBy(c => c.PartOfWorld)
                        .SelectMany(g => g.OrderBy(c => c.Population).Take(3).Select(c => $"{g.Key}: {c.Name} ({c.Population})"));
                    WriteReport(top3MinByPart, toFile);
                    break;
                case "11":
                    var top3MaxByPart = dbCountries.GroupBy(c => c.PartOfWorld)
                        .SelectMany(g => g.OrderByDescending(c => c.Population).Take(3).Select(c => $"{g.Key}: {c.Name} ({c.Population})"));
                    WriteReport(top3MaxByPart, toFile);
                    break;
                case "12":
                    Console.Write("Введите страну: ");
                    string cName3 = Console.ReadLine().ToLower();
                    var c3 = dbCountries.FirstOrDefault(c => c.Name.ToLower() == cName3);
                    if (c3 != null) WriteReport(new[] { $"Среднее население городов: {(long)c3.Cities.Average(city => city.Population)}" }, toFile);
                    else WriteReport(new[] { "Не найдено" }, toFile);
                    break;
                case "13":
                    Console.Write("Введите страну: ");
                    string cName4 = Console.ReadLine().ToLower();
                    var c4 = dbCountries.FirstOrDefault(c => c.Name.ToLower() == cName4);
                    if (c4 != null && c4.Cities.Any())
                    {
                        var minCity = c4.Cities.OrderBy(city => city.Population).First();
                        WriteReport(new[] { $"Самый маленький город: {minCity.Name} ({minCity.Population} чел.)" }, toFile);
                    }
                    else WriteReport(new[] { "Не найдено или нет городов" }, toFile);
                    break;
            }
        }

        // Вспомогательный метод для вывода на экран или в файл
        static void WriteReport(IEnumerable<string> lines, bool toFile)
        {
            if (toFile)
            {
                File.WriteAllLines("report.txt", lines);
                Console.WriteLine("Отчет сохранен в файл 'report.txt' (в папке с exe-файлом проекта).");
            }
            else
            {
                foreach (var line in lines) Console.WriteLine(line);
            }
        }

        // Тестовые данные
        static void InitData()
        {
            dbCountries.Add(new Country { Name = "Россия", Capital = "Москва", Population = 146000000, Area = 17098246, PartOfWorld = "Европа", Cities = new List<City> { new City { Name = "Москва", Population = 12500000 }, new City { Name = "Санкт-Петербург", Population = 5300000 }, new City { Name = "Новосибирск", Population = 1600000 } } });
            dbCountries.Add(new Country { Name = "Франция", Capital = "Париж", Population = 67000000, Area = 551695, PartOfWorld = "Европа", Cities = new List<City> { new City { Name = "Париж", Population = 2100000 }, new City { Name = "Марсель", Population = 860000 } } });
            dbCountries.Add(new Country { Name = "Китай", Capital = "Пекин", Population = 1400000000, Area = 9596961, PartOfWorld = "Азия", Cities = new List<City> { new City { Name = "Пекин", Population = 21500000 }, new City { Name = "Шанхай", Population = 24000000 } } });
            dbCountries.Add(new Country { Name = "Египет", Capital = "Каир", Population = 100000000, Area = 1002450, PartOfWorld = "Африка", Cities = new List<City> { new City { Name = "Каир", Population = 9500000 }, new City { Name = "Александрия", Population = 5000000 } } });
            dbCountries.Add(new Country { Name = "Алжир", Capital = "Алжир", Population = 43000000, Area = 2381741, PartOfWorld = "Африка", Cities = new List<City> { new City { Name = "Алжир", Population = 2600000 } } });
        }
    }
}