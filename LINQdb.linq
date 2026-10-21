<Query Kind="Program" />

void Main()
{
    // Имитация нормализованной базы данных "Страны"
    var countries = new List<Country>
    {
        new Country { Id = 1, Name = "Россия", PartOfWorld = "Европа/Азия", Area = 17098246, Population = 146000000, 
            Capital = new City { Name = "Москва", Population = 12500000 } },
        new Country { Id = 2, Name = "Франция", PartOfWorld = "Европа", Area = 551695, Population = 67000000, 
            Capital = new City { Name = "Париж", Population = 2100000 } },
        new Country { Id = 3, Name = "Германия", PartOfWorld = "Европа", Area = 357022, Population = 83000000, 
            Capital = new City { Name = "Берлин", Population = 3600000 } },
        new Country { Id = 4, Name = "Китай", PartOfWorld = "Азия", Area = 9596961, Population = 1400000000, 
            Capital = new City { Name = "Пекин", Population = 21500000 } },
        new Country { Id = 5, Name = "США", PartOfWorld = "Северная Америка", Area = 9833517, Population = 331000000, 
            Capital = new City { Name = "Вашингтон", Population = 700000 } }
    };

    var cities = new List<City>
    {
        new City { Name = "Москва", Population = 12500000, CountryId = 1 },
        new City { Name = "Санкт-Петербург", Population = 5300000, CountryId = 1 },
        new City { Name = "Новосибирск", Population = 1600000, CountryId = 1 },
        new City { Name = "Париж", Population = 2100000, CountryId = 2 },
        new City { Name = "Марсель", Population = 860000, CountryId = 2 },
        new City { Name = "Берлин", Population = 3600000, CountryId = 3 },
        new City { Name = "Гамбург", Population = 1800000, CountryId = 3 },
        new City { Name = "Пекин", Population = 21500000, CountryId = 4 },
        new City { Name = "Шанхай", Population = 24000000, CountryId = 4 }
    };

    Console.WriteLine("--- Задание 2 ---");
    // Отобразить всю информацию о странах
    countries.ForEach(c => Console.WriteLine($"{c.Name} | {c.Capital.Name} | Площадь: {c.Area} | Население: {c.Population}"));

    // Отобразить название стран
    countries.Select(c => c.Name).Dump("Названия стран");

    // Отобразить название столиц
    countries.Select(c => c.Capital.Name).Dump("Названия столиц");

    // Отобразить название крупных городов конкретной страны (например, России - Id=1)
    cities.Where(c => c.CountryId == 1).Select(c => c.Name).Dump("Города России");

    // Отобразить название столиц с количеством жителей больше пяти миллионов
    countries.Where(c => c.Capital.Population > 5000000).Select(c => c.Capital.Name).Dump("Столицы > 5 млн");

    // Отобразить название всех европейских стран
    countries.Where(c => c.PartOfWorld.Contains("Европа")).Select(c => c.Name).Dump("Европейские страны");

    // Отобразить название стран с площадью большей конкретного числа (например, 1 000 000)
    countries.Where(c => c.Area > 1000000).Select(c => c.Name).Dump("Страны площадью > 1 млн");


    Console.WriteLine("--- Задание 3 ---");
    // Отобразить все столицы, у которых в названии есть буквы a, р
    countries.Where(c => c.Capital.Name.ToLower().Contains("а") && c.Capital.Name.ToLower().Contains("р"))
             .Select(c => c.Capital.Name).Dump("Столицы с буквами 'а' и 'р'");

    // Отобразить все столицы, у которых название начинается с буквы k
    countries.Where(c => c.Capital.Name.StartsWith("К") || c.Capital.Name.StartsWith("K"))
             .Select(c => c.Capital.Name).Dump("Столицы на 'К'");

    // Отобразить название стран, у которых площадь находится в указанном диапазоне (например, от 300к до 1 млн)
    countries.Where(c => c.Area >= 300000 && c.Area <= 1000000).Select(c => c.Name).Dump("Площадь 300к - 1 млн");

    // Отобразить название стран, у которых количество жителей больше указанного числа (например, 80 млн)
    countries.Where(c => c.Population > 80000000).Select(c => c.Name).Dump("Население > 80 млн");


    Console.WriteLine("--- Задание 4 ---");
    // Топ-5 стран по площади
    countries.OrderByDescending(c => c.Area).Take(5).Select(c => c.Name).Dump("Топ-5 по площади");

    // Топ-5 столиц по количеству жителей
    countries.OrderByDescending(c => c.Capital.Population).Take(5).Select(c => c.Capital.Name).Dump("Топ-5 столиц по населению");

    // Страна с самой большой площадью
    countries.OrderByDescending(c => c.Area).FirstOrDefault()?.Name.Dump("Самая большая страна");

    // Столица с самым большим количеством жителей
    countries.OrderByDescending(c => c.Capital.Population).FirstOrDefault()?.Capital.Name.Dump("Самая большая столица");

    // Страна с самой маленькой площадью в Европе
    countries.Where(c => c.PartOfWorld.Contains("Европа"))
             .OrderBy(c => c.Area).FirstOrDefault()?.Name.Dump("Самая маленькая страна в Европе");

    // Средняя площадь стран в Европе
    countries.Where(c => c.PartOfWorld.Contains("Европа")).Average(c => c.Area).Dump("Средняя площадь в Европе");

    // Топ-3 городов по количеству жителей для конкретной страны (например, Китай - Id=4)
    cities.Where(c => c.CountryId == 4).OrderByDescending(c => c.Population).Take(3).Select(c => c.Name).Dump("Топ-3 города Китая");

    // Общее количество стран
    countries.Count().Dump("Всего стран");

    // Часть света с максимальным количеством стран
    countries.GroupBy(c => c.PartOfWorld)
             .OrderByDescending(g => g.Count())
             .FirstOrDefault()?.Key.Dump("Часть света с макс. кол-вом стран");

    // Количество стран в каждой части света
    countries.GroupBy(c => c.PartOfWorld)
             .Select(g => new { ЧастьСвета = g.Key, Количество = g.Count() })
             .Dump("Стран в каждой части света");
}

// Классы для имитации БД
public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PartOfWorld { get; set; }
    public long Area { get; set; }
    public long Population { get; set; }
    public City Capital { get; set; }
}

public class City
{
    public string Name { get; set; }
    public long Population { get; set; }
    public int CountryId { get; set; }
}