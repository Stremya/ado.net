using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class Student
{
    public string Name { get; set; }
    public string Group { get; set; }
    public int Age { get; set; }
    public double GPA { get; set; }
    public int PassedCredits { get; set; }

    public override string ToString()
    {
        return $"{Name,-10} | {Group,-6} | {Age,2} | {GPA,4:F1} | {PassedCredits,2}";
    }
}

class Program
{
    private static List<Student> GetStudents()
    {
        return new List<Student>
        {
            new Student { Name = "Алексей", Group = "ИТ-41", Age = 20, GPA = 4.2, PassedCredits = 40 },
            new Student { Name = "Борис", Group = "ИТ-41", Age = 19, GPA = 3.1, PassedCredits = 35 },
            new Student { Name = "Виктория", Group = "ПИ-42", Age = 21, GPA = 4.8, PassedCredits = 45 },
            new Student { Name = "Григорий", Group = "ИБ-43", Age = 20, GPA = 2.9, PassedCredits = 20 },
            new Student { Name = "Дарья", Group = "ИТ-41", Age = 22, GPA = 4.5, PassedCredits = 42 },
            new Student { Name = "Елена", Group = "ПИ-42", Age = 20, GPA = 3.8, PassedCredits = 38 },
            new Student { Name = "Жанна", Group = "ИБ-43", Age = 23, GPA = 4.9, PassedCredits = 48 },
            new Student { Name = "Игорь", Group = "ИТ-41", Age = 19, GPA = 2.5, PassedCredits = 15 }
        };
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Task1_QuerySyntax();
        Task2_MethodSyntax();
        Task3_Agreement();
        Task4_Xml();

        Console.WriteLine("\nНажмите Enter для выхода...");
        Console.ReadLine();
    }

    static void Task1_QuerySyntax()
    {
        var students = GetStudents();

        var result = from s in students
                     where s.Group == "ИТ-41" && s.GPA > 3.5
                     orderby s.Age
                     select s;

        Console.WriteLine("--- Задание 1: Студенты ИТ-41 с баллом > 3.5 ---");
        Console.WriteLine("Имя        | Группа | Возраст | Балл | Зачеты");
        foreach (var s in result)
        {
            Console.WriteLine(s.ToString());
        }
        Console.WriteLine();
    }

    static void Task2_MethodSyntax()
    {
        var students = GetStudents();

        var result = students
            .Where(s => s.GPA > 3.5) 
            .OrderByDescending(s => s.GPA)
            .Select(s => new
            {
                FullName = s.Name,
                Scholarship = s.GPA >= 4.7 ? "Ректорская" :
                              s.GPA >= 4.5 ? "Повышенная" :
                              s.GPA >= 4.0 ? "Стандартная" : "Нет"
            });

        Console.WriteLine("--- Задание 2: Стипендии (синтаксис методов) ---");
        foreach (var item in result)
        {
            Console.WriteLine($"{item.FullName}: {item.Scholarship}");
        }
        Console.WriteLine();
    }

    static void Task3_Agreement()
    {
        var students = GetStudents();
        Console.WriteLine("--- Задание 3: Статистика ---");

        var groups = from s in students
                     group s by s.Group into g
                     orderby g.Key
                     select new
                     {
                         GroupName = g.Key,
                         AvgGPA = g.Average(s => s.GPA)
                     };

        foreach (var g in groups)
        {
            Console.WriteLine($"Группа {g.GroupName}: Средний балл {g.AvgGPA:F2}");
        }

        var oldestWith45 = students
            .Where(s => s.PassedCredits == 45)
            .OrderByDescending(s => s.Age)
            .FirstOrDefault();

        Console.WriteLine();
        if (oldestWith45 != null)
            Console.WriteLine($"Студент с 45 зачетами: {oldestWith45.Name} ({oldestWith45.Age} год)");
        else
            Console.WriteLine("Студентов с 45 зачетами не найдено.");

        Console.WriteLine();
    }

    static void Task4_Xml()
    {
          
        XDocument doc = new XDocument(
            new XElement("university",
                new XElement("course", new XAttribute("name", "Программирование"),
                    new XElement("student", new XAttribute("name", "Кирилл"), new XAttribute("gra", "3.9")),
                    new XElement("student", new XAttribute("name", "Лариса"), new XAttribute("gra", "4.7")),
                    new XElement("student", new XAttribute("name", "Михаил"), new XAttribute("gra", "3.2")),
                    new XElement("student", new XAttribute("name", "Наталья"), new XAttribute("gra", "4.1"))
                )
            )
        );
        doc.Save("uni.xml");

        XDocument loadedDoc = XDocument.Load("uni.xml");

        var excellentStudents = from st in loadedDoc.Descendants("student")
                                where (double)st.Attribute("gra") > 4.0
                                select st.Attribute("name").Value;

        Console.WriteLine("--- Задание 4: Отличники из XML ---");
        foreach (var name in excellentStudents)
        {
            Console.WriteLine(name);
        }
        Console.WriteLine();
    }
}