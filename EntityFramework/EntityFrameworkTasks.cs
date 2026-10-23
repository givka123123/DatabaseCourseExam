using DatabaseCourseExam.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseCourseExam.EntityFramework;

public static class EntityFrameworkTasks
{
    // TODO (задание 2): Вывести студентов с названием группы;
    public static async Task ShowStudentsAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        List<Student> students = await context.Students
            .AsNoTracking()
            .Include(student => student.Group)
            .OrderBy(student => student.Id)
            .ToListAsync();

        foreach (Student student in students)
        {
            string groupName = student.Group?.Name ?? "группа не назначена";

            Console.WriteLine(
                $"{student.Id}. {student.FirstName} {student.LastName}, " +
                $"возраст: {student.Age}, группа: {groupName}");
        }
    }

    public static async Task UpdateStudentAgeAsync(
        int studentId,
        int newAge)
    {
        await using var context = SchoolDbContextFactory.Create();

        Student? student = await context.Students
            .FirstOrDefaultAsync(x => x.Id == studentId);

        if (student is null)
        {
            Console.WriteLine("Студент не найден.");
            return;
        }

        student.Age = newAge;

        int affectedRows = await context.SaveChangesAsync();

        Console.WriteLine(
            $"Возраст изменён. Сохранено изменений: {affectedRows}");
    }

    // TODO (задание 6): Выбирать только нужные поля через Select
    public static async Task ShowCoursesAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        var courses = await context.Courses
            .AsNoTracking()
            .OrderBy(course => course.Name)
            .Select(course => new
            {
                course.Name,
                course.Price,
                course.AvailablePlaces,

                NoPlaces = course.AvailablePlaces == 0
            })
            .ToListAsync();

        foreach (var course in courses)
        {
            string placesInfo = course.NoPlaces
                ? "мест нет"
                : $"свободных мест: {course.AvailablePlaces}";

            Console.WriteLine(
                $"{course.Name} | Цена: {course.Price:N2} | {placesInfo}");
        }
    }
}