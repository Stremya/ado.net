using CarRentalDbFirst.CarRentalDbModel; // ВАЖНО: Добавляем пространство имен вашей модели!
using System;
using System.Data.Entity; // Необходимо для работы с DbContext
using System.Linq;

namespace CarRentalDbFirst
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== ЛАБОРАТОРНАЯ РАБОТА: АВТОПАРК ===\n");

            // Раскомментируйте вызовы методов, как просят в задании:

            Task1_AddInitialData(); // Сначала запустите это, чтобы заполнить БД 
            Task2_GetAllCars();
            Task3_FindCarByModel("Camry");
            Task4_GetCarById(1);
            Task5_GetCarsWithBrandInfo();
            Task6_AddNewCar("X5", 2023, 150.00m, 1);
            Task7_UpdateCarPrice(1, 200.00m);

            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }

        //====================================================
        // ЗАДАНИЕ 0. Заполнение базы данных
        //====================================================
        static void Task1_AddInitialData()
        {
            Console.WriteLine("Добавление начальных данных...");
            using (CarRentalEntities db = new CarRentalEntities())
            {
                if (!db.Brands.Any())
                {
                    var toyota = new Brand { Name = "Toyota", Country = "Япония" };
                    var bmw = new Brand { Name = "BMW", Country = "Германия" };

                    db.Brands.Add(toyota);
                    db.Brands.Add(bmw);

                    var car1 = new Car { Model = "Camry", ReleaseYear = 2020, PricePerDay = 50.00m, Brand = toyota };
                    var car2 = new Car { Model = "Corolla", ReleaseYear = 2019, PricePerDay = 40.00m, Brand = toyota };
                    var car3 = new Car { Model = "X5", ReleaseYear = 2022, PricePerDay = 100.00m, Brand = bmw };

                    db.Cars.Add(car1);
                    db.Cars.Add(car2);
                    db.Cars.Add(car3);

                    db.SaveChanges();
                    Console.WriteLine("Данные успешно добавлены!");
                }
                else
                {
                    Console.WriteLine("База данных уже содержит записи.");
                }
            }
        }

        // ЗАДАНИЕ 1.
        static void Task2_GetAllCars()
        {
            Console.WriteLine("\n--- ЗАДАНИЕ 1: Все автомобили по алфавиту ---");
            using (CarRentalEntities db = new CarRentalEntities())
            {
                var cars = db.Cars.OrderBy(c => c.Model).ToList();
                foreach (var car in cars)
                {
                    Console.WriteLine($"Модель: {car.Model}, Цена за день: {car.PricePerDay}");
                }
            }
        }

        // ЗАДАНИЕ 2.
        static void Task3_FindCarByModel(string modelName)
        {
            Console.WriteLine($"\n--- ЗАДАНИЕ 2: Поиск машины '{modelName}' ---");
            using (CarRentalEntities db = new CarRentalEntities())
            {
                var car = db.Cars.FirstOrDefault(c => c.Model == modelName);
                if (car != null)
                    Console.WriteLine($"Найдена модель: {car.Model}, Год выпуска: {car.ReleaseYear}");
                else
                    Console.WriteLine("Машина не найдена");
            }
        }

        // ЗАДАНИЕ 3.
        static void Task4_GetCarById(int id)
        {
            Console.WriteLine($"\n--- ЗАДАНИЕ 3: Поиск по ID ({id}) ---");
            using (CarRentalEntities db = new CarRentalEntities())
            {
                var car = db.Cars.Find(id);
                if (car != null)
                    Console.WriteLine($"Машина с ID {id} найдена: {car.Model}");
                else
                    Console.WriteLine("Машина с таким ID не найдена.");
            }
        }

        // ЗАДАНИЕ 4.
        static void Task5_GetCarsWithBrandInfo()
        {
            Console.WriteLine("\n--- ЗАДАНИЕ 4: Автомобили с названием марки ---");
            using (CarRentalEntities db = new CarRentalEntities())
            {
                var cars = db.Cars.ToList();
                foreach (var car in cars)
                {
                    Console.WriteLine($"Модель: {car.Model} | Марка: {car.Brand.Name} | Страна: {car.Brand.Country}");
                }
            }
        }

        // ЗАДАНИЕ 5.
        static void Task6_AddNewCar(string model, int year, decimal price, int brandId)
        {
            Console.WriteLine($"\n--- ЗАДАНИЕ 5: Добавление машины '{model}' ---");
            using (CarRentalEntities db = new CarRentalEntities())
            {
                var newCar = new Car { Model = model, ReleaseYear = year, PricePerDay = price, BrandId = brandId };
                db.Cars.Add(newCar);
                db.SaveChanges();
                Console.WriteLine($"Машина {model} успешно добавлена в базу!");
            }
        }

        // ЗАДАНИЕ 6.
        static void Task7_UpdateCarPrice(int id, decimal newPrice)
        {
            Console.WriteLine($"\n--- ЗАДАНИЕ 6: Обновление цены машины (ID: {id}) ---");
            using (CarRentalEntities db = new CarRentalEntities())
            {
                var car = db.Cars.Find(id);
                if (car != null)
                {
                    car.PricePerDay = newPrice;
                    db.SaveChanges();
                    Console.WriteLine($"Цена для машины {car.Model} обновлена на {newPrice}!");
                }
                else
                {
                    Console.WriteLine("Машина с таким ID не найдена.");
                }
            }
        }
    }
}