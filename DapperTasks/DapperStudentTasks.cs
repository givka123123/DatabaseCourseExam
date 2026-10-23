using DatabaseCourseExam.Data;
using Dapper;
using DatabaseCourseExam.DapperTasks.DTO;

namespace DatabaseCourseExam.DapperTasks;

public static class DapperStudentTasks
{
    // Задание 5:
    public static async Task ShowStudentsAsync()
    {
        Console.Write("Введите ID группы: ");
        string? input = Console.ReadLine();

        if (!int.TryParse(input, out int groupId) || groupId <= 0)
        {
            Console.WriteLine("Некорректный ID группы.");
            return;
        }

        await using var connection = DatabaseConnection.Create();

        const string sql = """
            SELECT
                s.id AS Id,
                s.first_name AS FirstName,
                s.last_name AS LastName,
                s.age AS Age,
                g.name AS GroupName
            FROM students AS s
            JOIN student_groups AS g ON g.id = s.group_id
            WHERE s.group_id = @GroupId
            ORDER BY s.id;
            """;

        var students = await connection.QueryAsync<StudentDto>(
            sql,
            new { GroupId = groupId });

        bool found = false;

        foreach (StudentDto student in students)
        {
            found = true;

            Console.WriteLine(
                $"{student.Id}. {student.FirstName} {student.LastName}, " +
                $"возраст: {student.Age}, группа: {student.GroupName}");
        }

        if (!found)
        {
            Console.WriteLine("Студенты в указанной группе не найдены.");
        }
    }

    public static async Task UpdateStudentAgeAsync(
        int studentId,
        int newAge)
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            UPDATE students
            SET age = @NewAge
            WHERE id = @StudentId;
            """;

        int affectedRows = await connection.ExecuteAsync(
            sql,
            new
            {
                NewAge = newAge,
                StudentId = studentId
            });

        Console.WriteLine(
            affectedRows == 0
                ? "Студент не найден."
                : $"Изменено записей: {affectedRows}");
    }

    // Задание 4:
    public static async Task ShowCoursesAsync()
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            SELECT
                c.id AS Id,
                c.name AS Name,
                c.price AS Price,
                c.available_places AS AvailablePlaces,
                CAST(COUNT(sc.student_id) AS integer) AS EnrolledCount
            FROM courses AS c
            LEFT JOIN student_courses AS sc ON sc.course_id = c.id
            GROUP BY
                c.id,
                c.name,
                c.price,
                c.available_places
            ORDER BY c.name;
            """;

        var courses = await connection.QueryAsync<CourseDto>(sql);

        bool found = false;

        foreach (CourseDto course in courses)
        {
            found = true;

            Console.WriteLine(
                $"{course.Name} | " +
                $"Цена: {course.Price:N2} | " +
                $"Свободных мест: {course.AvailablePlaces} | " +
                $"Записано студентов: {course.EnrolledCount}");
        }

        if (!found)
        {
            Console.WriteLine("Курсы не найдены.");
        }
    }

    public static async Task EnrollStudentAsync(
        int studentId,
        int courseId)
    {
        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            const string updateSql = """
                UPDATE courses
                SET available_places = available_places - 1
                WHERE id = @CourseId
                  AND available_places > 0
                RETURNING available_places;
                """;

            int? remainingPlaces =
                await connection.QuerySingleOrDefaultAsync<int?>(
                    updateSql,
                    new { CourseId = courseId },
                    transaction);

            if (remainingPlaces is null)
            {
                throw new InvalidOperationException(
                    "Курс не найден или свободных мест нет.");
            }

            const string insertSql = """
                INSERT INTO student_courses (student_id, course_id)
                VALUES (@StudentId, @CourseId);
                """;

            await connection.ExecuteAsync(
                insertSql,
                new
                {
                    StudentId = studentId,
                    CourseId = courseId
                },
                transaction);

            await transaction.CommitAsync();

            Console.WriteLine(
                $"Студент записан на курс. Осталось мест: {remainingPlaces}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Операция отменена: {ex.Message}");
        }
    }
}