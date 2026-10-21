<Query Kind="Program" />

void Main()
{
    var countries = new List<Country>
    {
        new Country { Name = "Россия", Capital = "Москва", Population = 146000000, Area = 17098246, PartOfWorld = "Европа/Азия" },
        new Country { Name = "Франция", Capital = "Париж", Population = 67000000, Area = 551695, PartOfWorld = "Европа" },
        new Country { Name = "Германия", Capital = "Берлин", Population = 83000000, Area = 357022, PartOfWorld = "Европа" },
        new Country { Name = "Китай", Capital = "Пекин", Population = 1400000000, Area = 9596961, PartOfWorld = "Азия" },
        new Country { Name = "Египет", Capital = "Каир", Population = 100000000, Area = 1002450, PartOfWorld = "Африка" },
        new Country { Name = "США", Capital = "Вашингтон", Population = 331000000, Area = 9833517, PartOfWorld = "Северная Америка" },
        new Country { Name = "Алжир", Capital = "Алжир", Population = 43000000, Area = 2381741, PartOfWorld = "Африка" }
    };

    Console.WriteLine("--- ЗАДАНИЕ 2 ---");
    
    var allInfo = from c in countries select c;
    allInfo.Dump("Вся информация о странах");

    var names = from c in countries select c.Name;
    names.Dump("Названия стран");

    var capitals = from c in countries select c.Capital;
    capitals.Dump("Названия столиц");

    var europe = from c in countries 
                 where c.PartOfWorld.Contains("Европа") 
                 select c.Name;
    europe.Dump("Европейские страны");

    var bigArea = from c in countries 
                  where c.Area > 1000000 
                  select c.Name;
    bigArea.Dump("Страны с площадью > 1 млн");

    Console.WriteLine("--- ЗАДАНИЕ 3 ---");

    var letters = from c in countries 
                  where c.Name.ToLower().Contains("а") && c.Name.ToLower().Contains("и") 
                  select c.Name;
    letters.Dump("Страны, в названии которых есть 'а' и 'и'");

    var startsWithA = from c in countries 
                      where c.Name.ToLower().StartsWith("а") 
                      select c.Name;
    startsWithA.Dump("Страны на букву 'А'");

    var areaRange = from c in countries 
                    where c.Area >= 300000 && c.Area <= 1500000 
                    select c.Name;
    areaRange.Dump("Площадь в диапазоне 300к - 1.5 млн");

    var bigPop = from c in countries 
                 where c.Population > 80000000 
                 select c.Name;
    bigPop.Dump("Население > 80 млн");

    Console.WriteLine("--- ЗАДАНИЕ 4 ---");

    var top5Area = (from c in countries 
                    orderby c.Area descending 
                    select c.Name).Take(5);
    top5Area.Dump("Топ-5 по площади");

    var top5Pop = (from c in countries 
                   orderby c.Population descending 
                   select c.Name).Take(5);
    top5Pop.Dump("Топ-5 по населению");

    var maxArea = (from c in countries 
                   orderby c.Area descending 
                   select c.Name).FirstOrDefault();
    maxArea.Dump("Самая большая страна");

    var maxPop = (from c in countries 
                  orderby c.Population descending 
                  select c.Name).FirstOrDefault();
    maxPop.Dump("Самая населенная страна");

    var minAreaAfrica = (from c in countries 
                         where c.PartOfWorld == "Африка" 
                         orderby c.Area ascending 
                         select c.Name).FirstOrDefault();
    minAreaAfrica.Dump("Самая маленькая страна в Африке");

    var avgAreaAsia = (from c in countries 
                       where c.PartOfWorld == "Азия" 
                       select c.Area).Average();
    avgAreaAsia.Dump("Средняя площадь в Азии");
}

public class Country
{
    public string Name { get; set; }
    public string Capital { get; set; }
    public long Population { get; set; }
    public long Area { get; set; }
    public string PartOfWorld { get; set; }
}