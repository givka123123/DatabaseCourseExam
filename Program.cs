
using DatabaseCourseExam.AdoNet;
using System.Data;
using DatabaseCourseExam.EntityFramework;
using DatabaseCourseExam.DapperTasks;

DataTable? studentsTable = null;
/*
 1. ExecuteReader, ExecuteScalar, ExecuteNonQuery:  
   Первый читает много строк, второй — ровно одно значение (например, `COUNT`), третий — делает `INSERT`/`UPDATE`/`DELETE` и ничего не возвращает

11. Как EF Core узнаёт об изменениях:
   EF сам следит за всеми загруженными объектами. Поменял свойство — он это запомнил, а при `SaveChanges()` сам сгенерирует нужный `UPDATE`

15. Создаёт ли async новый поток:  
   Нет. Async просто освобождает текущий поток, пока ждёт долгую операцию (например, ответ от базы), но сам по себе новые потоки не плодит
 */

bool TryReadInt(string prompt, out int value)
{
    Console.Write(prompt);
    return int.TryParse(Console.ReadLine(), out value);
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Зачёт по работе с БД ===");
    Console.WriteLine("1. Показать студентов");
    Console.WriteLine("2. Изменить возраст студента");
    Console.WriteLine("3. Записать студента на курс");
    Console.WriteLine("4. Загрузить студентов в DataTable");
    Console.WriteLine("5. Изменить возраст в DataTable");
    Console.WriteLine("6. Показать DataTable");
    Console.WriteLine("7. Сохранить изменения DataTable");
    Console.WriteLine("8. EF Core: показать студентов");
    Console.WriteLine("9. EF Core: изменить возраст студента");
    Console.WriteLine("10. EF Core: показать курсы");
    Console.WriteLine("11. Dapper: показать студентов");
    Console.WriteLine("12. Dapper: изменить возраст");
    Console.WriteLine("13. Dapper: показать курсы");
    Console.WriteLine("14. Dapper: записать студента на курс");
    Console.WriteLine("0. Выход");
    Console.Write("Выберите действие: ");

    string? choice = Console.ReadLine()?.Trim();

    try
    {
        switch (choice)
        {
            case "1":
                await AdoNetTasks.ShowStudentsAsync();
                break;

            case "2":
                {
                    if (!TryReadInt("ID студента: ", out int studentId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        break;
                    }

                    if (!TryReadInt("Новый возраст: ", out int age))
                    {
                        Console.WriteLine("Некорректный возраст.");
                        break;
                    }

                    if (age is < 14 or > 100)
                    {
                        Console.WriteLine("Возраст должен быть от 14 до 100.");
                        break;
                    }

                    await AdoNetTasks.UpdateStudentAgeAsync(studentId, age);
                    break;
                }

            case "3":
                {
                    if (!TryReadInt("ID студента: ", out int enrollmentStudentId))
                    {
                        Console.WriteLine("Некорректный ID студента.");
                        break;
                    }

                    if (!TryReadInt("ID курса: ", out int courseId))
                    {
                        Console.WriteLine("Некорректный ID курса.");
                        break;
                    }

                    await AdoNetTasks.EnrollStudentAsync(
                        enrollmentStudentId,
                        courseId);

                    break;
                }

            case "4":
                studentsTable = await DisconnectedTasks.LoadStudentsAsync();
                Console.WriteLine("Данные загружены в память.");
                DisconnectedTasks.PrintTable(studentsTable);
                break;

            case "5":
                {
                    if (studentsTable is null)
                    {
                        Console.WriteLine("Сначала загрузите данные (пункт 4).");
                        break;
                    }

                    if (!TryReadInt("ID студента: ", out int disconnectedStudentId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        break;
                    }

                    if (!TryReadInt("Новый возраст: ", out int disconnectedAge))
                    {
                        Console.WriteLine("Некорректный возраст.");
                        break;
                    }

                    if (disconnectedAge is < 14 or > 100)
                    {
                        Console.WriteLine("Возраст должен быть от 14 до 100.");
                        break;
                    }

                    DisconnectedTasks.ChangeAgeInMemory(
                        studentsTable,
                        disconnectedStudentId,
                        disconnectedAge);

                    break;
                }

            case "6":
                if (studentsTable is null)
                {
                    Console.WriteLine("Сначала загрузите данные (пункт 4).");
                    break;
                }

                DisconnectedTasks.PrintTable(studentsTable);
                break;

            case "7":
                if (studentsTable is null)
                {
                    Console.WriteLine("Сначала загрузите данные (пункт 4).");
                    break;
                }

                DisconnectedTasks.SaveChanges(studentsTable);
                break;

            case "8":
                await EntityFrameworkTasks.ShowStudentsAsync();
                break;

            case "9":
                {
                    if (!TryReadInt("ID студента: ", out int efStudentId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        break;
                    }

                    if (!TryReadInt("Новый возраст: ", out int efNewAge))
                    {
                        Console.WriteLine("Некорректный возраст.");
                        break;
                    }

                    if (efNewAge is < 14 or > 100)
                    {
                        Console.WriteLine("Возраст должен быть от 14 до 100.");
                        break;
                    }

                    await EntityFrameworkTasks.UpdateStudentAgeAsync(
                        efStudentId,
                        efNewAge);

                    break;
                }

            case "10":
                await EntityFrameworkTasks.ShowCoursesAsync();
                break;

            case "11":
                await DapperStudentTasks.ShowStudentsAsync();
                break;

            case "12":
                {
                    if (!TryReadInt("ID студента: ", out int dapperStudentId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        break;
                    }

                    if (!TryReadInt("Новый возраст: ", out int dapperNewAge))
                    {
                        Console.WriteLine("Некорректный возраст.");
                        break;
                    }

                    if (dapperNewAge is < 14 or > 100)
                    {
                        Console.WriteLine("Возраст должен быть от 14 до 100.");
                        break;
                    }

                    await DapperStudentTasks.UpdateStudentAgeAsync(
                        dapperStudentId,
                        dapperNewAge);

                    break;
                }

            case "13":
                await DapperStudentTasks.ShowCoursesAsync();
                break;

            case "14":
                {
                    if (!TryReadInt("ID студента: ", out int dapperEnrollStudentId))
                    {
                        Console.WriteLine("Некорректный ID студента.");
                        break;
                    }

                    if (!TryReadInt("ID курса: ", out int dapperCourseId))
                    {
                        Console.WriteLine("Некорректный ID курса.");
                        break;
                    }

                    await DapperStudentTasks.EnrollStudentAsync(
                        dapperEnrollStudentId,
                        dapperCourseId);

                    break;
                }

            case "0":
                return;

            default:
                Console.WriteLine("Неизвестная команда.");
                break;
        }
    }
    catch (FormatException)
    {
        Console.WriteLine("Введите целое число.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}
