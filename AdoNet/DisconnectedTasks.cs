using System.Data;
using DatabaseCourseExam.Data;
using Npgsql;
using NpgsqlTypes;

namespace DatabaseCourseExam.AdoNet;

public static class DisconnectedTasks
{
    public static async Task<DataTable> LoadStudentsAsync()
    {
        var table = new DataTable("Students");

        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        const string sql = """
            SELECT id, first_name, last_name, age, group_id
            FROM students
            ORDER BY id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        using var adapter = new NpgsqlDataAdapter(command);

        adapter.Fill(table);

  
        table.PrimaryKey = new[] { table.Columns["id"]! };

        return table;
    }

    public static void PrintTable(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine(
                $"{row["id"]}: {row["first_name"]} " +
                $"{row["last_name"]}, возраст {row["age"]}");
        }
    }


    public static void ChangeAgeInMemory(
        DataTable table,
        int studentId,
        int newAge)
    {
        if (table.PrimaryKey.Length == 0)
        {
            Console.WriteLine("Ошибка: у таблицы не задан первичный ключ.");
            return;
        }

        DataRow? row = table.Rows.Find(studentId);

        if (row is null)
        {
            Console.WriteLine("Студент не найден в DataTable.");
            return;
        }

        int? currentAge = row["age"] is DBNull
            ? null
            : Convert.ToInt32(row["age"]);

        Console.WriteLine(
            $"До изменения: RowState = {row.RowState}, " +
            $"текущий возраст = {(currentAge?.ToString() ?? "NULL")}");

        if (currentAge == newAge)
        {
            Console.WriteLine(
                "Новое значение совпадает с текущим. " +
                "Строка не изменяется.");

            Console.WriteLine(
                $"После проверки: RowState = {row.RowState}");

            return;
        }

        row["age"] = newAge;

        Console.WriteLine(
            $"После изменения: RowState = {row.RowState}, " +
            $"новый возраст = {newAge}");
    }

    public static int SaveChanges(DataTable table)
    {
        using var connection = DatabaseConnection.Create();
        connection.Open();

        const string sql = """
            UPDATE students
            SET age = @age
            WHERE id = @id;
            """;

        using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(
            "@age",
            NpgsqlDbType.Integer,
            4,
            "age");

        command.Parameters.Add(
            "@id",
            NpgsqlDbType.Integer,
            4,
            "id");

        using var adapter = new NpgsqlDataAdapter();
        adapter.UpdateCommand = command;

        int affectedRows = adapter.Update(table);

        Console.WriteLine($"Сохранено изменений: {affectedRows}");

        return affectedRows;
    }
}