<Query Kind="Program" />

void Main()
{
    var countries = new List<Country>
    {
        new Country { Id = 1, Name = "Россия", PartOfWorld = "Европа/Азия", Area = 17098246 },
        new Country { Id = 2, Name = "Франция", PartOfWorld = "Европа", Area = 551695 },
        new Country { Id = 3, Name = "Китай", PartOfWorld = "Азия", Area = 9596961 },
        new Country { Id = 4, Name = "США", PartOfWorld = "Северная Америка", Area = 9833517 }
    };

    var cities = new List<City>
    {
        new City { Id = 1, CountryId = 1, Name = "Москва", Population = 12500000, IsCapital = true },
        new City { Id = 2, CountryId = 1, Name = "Санкт-Петербург", Population = 5300000, IsCapital = false },
        new City { Id = 3, CountryId = 2, Name = "Париж", Population = 2100000, IsCapital = true },
        new City { Id = 4, CountryId = 2, Name = "Марсель", Population = 860000, IsCapital = false },
        new City { Id = 5, CountryId = 3, Name = "Пекин", Population = 21500000, IsCapital = true },
        new City { Id = 6, CountryId = 3, Name = "Шанхай", Population = 24000000, IsCapital = false },
        new City { Id = 7, CountryId = 4, Name = "Вашингтон", Population = 700000, IsCapital = true },
        new City { Id = 8, CountryId = 4, Name = "Нью-Йорк", Population = 8400000, IsCapital = false }
    };

    int nextCityId = 9;

    bool exit = false;
    while (!exit)
    {
        Console.Clear();
        Console.WriteLine("=== БАЗА ДАННЫХ 'СТРАНЫ' ===");
        Console.WriteLine("1. Добавить страну (Задание 1)");
        Console.WriteLine("2. Добавить город/столицу (Задание 1)");
        Console.WriteLine("3. Обновить население города (Задание 2)");
        Console.WriteLine("4. Удалить город (Задание 3)");
        Console.WriteLine("5. Топ-3 стран по населению (Задание 4)");
        Console.WriteLine("6. Топ-3 столиц по населению (Задание 4)");
        Console.WriteLine("7. Страна с макс. населением (Задание 4)");
        Console.WriteLine("8. Город с макс. населением (Задание 4)");
        Console.WriteLine("0. Выход");
        Console.Write("Выбор: ");
        
        string choice = Console.ReadLine();
        Console.WriteLine();

        switch (choice)
        {
            case "1":
                Console.Write("Введите название страны: ");
                string cName = Console.ReadLine();
                Console.Write("Часть света: ");
                string part = Console.ReadLine();
                Console.Write("Площадь: ");
                long area = long.Parse(Console.ReadLine());
                countries.Add(new Country { Id = countries.Count + 1, Name = cName, PartOfWorld = part, Area = area });
                Console.WriteLine("Страна добавлена!");
                break;

            case "2":
                Console.Write("ID страны (для какой страны город): ");
                int cId = int.Parse(Console.ReadLine());
                Console.Write("Название города: ");
                string cityName = Console.ReadLine();
                Console.Write("Население: ");
                long pop = long.Parse(Console.ReadLine());
                Console.Write("Это столица? (y/n): ");
                bool isCap = Console.ReadLine().ToLower() == "y";
                
                cities.Add(new City { Id = nextCityId++, CountryId = cId, Name = cityName, Population = pop, IsCapital = isCap });
                Console.WriteLine("Город добавлен!");
                break;

            case "3":
                Console.Write("Введите название города для обновления: ");
                string updName = Console.ReadLine();
                var cityToUpdate = cities.FirstOrDefault(c => c.Name.ToLower() == updName.ToLower());
                if (cityToUpdate != null)
                {
                    Console.Write($"Новое население для {cityToUpdate.Name}: ");
                    cityToUpdate.Population = long.Parse(Console.ReadLine());
                    Console.WriteLine("Население обновлено!");
                }
                else Console.WriteLine("Город не найден.");
                break;

            case "4":
                Console.Write("Введите название города для удаления: ");
                string delName = Console.ReadLine();
                var cityToDelete = cities.FirstOrDefault(c => c.Name.ToLower() == delName.ToLower());
                if (cityToDelete != null)
                {
                    cities.Remove(cityToDelete);
                    Console.WriteLine("Город удален!");
                }
                else Console.WriteLine("Город не найден.");
                break;

            case "5":
                var topCountries = countries
                    .Select(c => new { 
                        Страна = c.Name, 
                        Население = cities.Where(city => city.CountryId == c.Id).Sum(city => city.Population) 
                    })
                    .OrderByDescending(x => x.Население)
                    .Take(3);
                topCountries.Dump("Топ-3 стран по населению");
                break;

            case "6":
                var topCapitals = cities
                    .Where(city => city.IsCapital)
                    .OrderByDescending(city => city.Population)
                    .Take(3)
                    .Select(city => new { Столица = city.Name, Население = city.Population });
                topCapitals.Dump("Топ-3 столиц по населению");
                break;

            case "7":
                var maxCountryPop = countries
                    .Select(c => new { 
                        Страна = c.Name, 
                        Население = cities.Where(city => city.CountryId == c.Id).Sum(city => city.Population) 
                    })
                    .OrderByDescending(x => x.Население)
                    .FirstOrDefault();
                maxCountryPop.Dump("Страна с максимальным населением");
                break;

            case "8":
                var maxCityPop = cities.OrderByDescending(city => city.Population).FirstOrDefault();
                maxCityPop.Dump("Город с максимальным населением");
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

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PartOfWorld { get; set; }
    public long Area { get; set; }
}

public class City
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public string Name { get; set; }
    public long Population { get; set; }
    public bool IsCapital { get; set; } 
}