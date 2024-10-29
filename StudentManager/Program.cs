using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudentManager
{

    // Класс для работы с базой данных MySQL
    public class StudentDatabaseManager
    {
        private string connectionString;

        public StudentDatabaseManager(string server, string database, string username, string password)
        {
            connectionString = $"Server={server};Database={database};Uid={username};Pwd={password};";
        }


        public void AddStudent(Student student)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var query = "INSERT INTO students (name, age, major) VALUES (@name, @age, @major)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", student.Name);
                    command.Parameters.AddWithValue("@age", student.Age);
                    command.Parameters.AddWithValue("@major", student.Major);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void RemoveStudent(int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var query = "DELETE FROM students WHERE id = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStudent(Student student)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var query = "UPDATE students SET name = @name, age = @age, major = @major WHERE id = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", student.Name);
                    command.Parameters.AddWithValue("@age", student.Age);
                    command.Parameters.AddWithValue("@major", student.Major);
                    command.Parameters.AddWithValue("@id", student.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Student> SearchStudents(string criteria, string value)
        {
            List<Student> students = new List<Student>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var query = $"SELECT * FROM students WHERE {criteria} LIKE @value";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value", "%" + value + "%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Age = reader.GetInt32("age"),
                                Major = reader.GetString("major")
                            });
                        }
                    }
                }
            }
            return students;
        }

        public List<Student> GetSortedStudents(string sortBy)
        {
            List<Student> students = new List<Student>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var query = $"SELECT * FROM students ORDER BY {sortBy}";
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            Age = reader.GetInt32("age"),
                            Major = reader.GetString("major")
                        });
                    }
                }
            }
            return students;
        }

        public void SaveToFile(string filePath)
        {
            var students = GetSortedStudents("id");
            var json = JsonConvert.SerializeObject(students, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void LoadFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var students = JsonConvert.DeserializeObject<List<Student>>(json);
                foreach (var student in students)
                {
                    AddStudent(student);
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var manager = new StudentDatabaseManager("localhost", "student_management", "root", "12345678");       
 
            while (true)
            {
                Console.WriteLine("Меню:");
                Console.WriteLine("1. Добавить студента");
                Console.WriteLine("2. Удалить студента");
                Console.WriteLine("3. Редактировать студента");
                Console.WriteLine("4. Поиск студентов");
                Console.WriteLine("5. Сортировка студентов");
                Console.WriteLine("6. Сохранить в файл");
                Console.WriteLine("7. Загрузить из файла");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите действие: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Введите имя: ");
                        var name = Console.ReadLine();
                        Console.Write("Введите возраст: ");
                        var age = int.Parse(Console.ReadLine());
                        Console.Write("Введите специальность: ");
                        var major = Console.ReadLine();
                        manager.AddStudent(new Student { Name = name, Age = age, Major = major });
                        break;

                    case "2":
                        Console.Write("Введите ID студента: ");
                        var idToDelete = int.Parse(Console.ReadLine());
                        manager.RemoveStudent(idToDelete);
                        break;

                    case "3":
                        Console.Write("Введите ID студента: ");
                        var idToEdit = int.Parse(Console.ReadLine());
                        Console.Write("Введите новое имя: ");
                        var newName = Console.ReadLine();
                        Console.Write("Введите новый возраст: ");
                        var newAge = int.Parse(Console.ReadLine());
                        Console.Write("Введите новую специальность: ");
                        var newMajor = Console.ReadLine();
                        manager.UpdateStudent(new Student { Id = idToEdit, Name = newName, Age = newAge, Major = newMajor });
                        break;

                    case "4":
                        Console.Write("Введите критерий (name, age, major): ");
                        var criteria = Console.ReadLine();
                        Console.Write("Введите значение для поиска: ");
                        var value = Console.ReadLine();
                        var searchResults = manager.SearchStudents(criteria, value);
                        foreach (var student in searchResults)
                            Console.WriteLine($"ID: {student.Id}, Имя: {student.Name}, Возраст: {student.Age}, Специальность: {student.Major}");
                        break;

                    case "5":
                        Console.Write("Введите критерий сортировки (name, age, major): ");
                        var sortBy = Console.ReadLine();
                        var sortedStudents = manager.GetSortedStudents(sortBy);
                        foreach (var student in sortedStudents)
                            Console.WriteLine($"ID: {student.Id}, Имя: {student.Name}, Возраст: {student.Age}, Специальность: {student.Major}");
                        break;

                    case "6":
                        var savePath = @"E:\students.json";
                        manager.SaveToFile(savePath);

                        break;

                    case "7":
                        Console.Write("Введите путь к файлу: ");
                        var loadPath = Console.ReadLine();
                        manager.LoadFromFile(loadPath);
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }
    }
}

