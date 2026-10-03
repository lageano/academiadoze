using System.Data.Common;
using System.Globalization;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DataReaderExtensions
{
    public static string GetStringValue(this DbDataReader reader, string columnName)
    {
        return reader[columnName].ToString()!;
    }

    public static string GetNullableString(this DbDataReader reader, string columnName, string defaultValue = "")
    {
        return reader[columnName] is DBNull ? defaultValue : reader[columnName].ToString()!;
    }

    public static byte[]? GetNullableBytes(this DbDataReader reader, string columnName)
    {
        return reader[columnName] is DBNull ? null : (byte[])reader[columnName];
    }

    public static int GetInt32Value(this DbDataReader reader, string columnName)
    {
        return Convert.ToInt32(reader[columnName]);
    }

    public static DateTime GetDateTimeValue(this DbDataReader reader, string columnName)
    {
        return Convert.ToDateTime(reader[columnName]);
    }

    public static DateOnly GetDateOnlyValue(this DbDataReader reader, string columnName)
    {
        return DateOnly.FromDateTime(Convert.ToDateTime(reader[columnName]));
    }

    // Cada SGBD devolve a coluna de hora em um tipo diferente: TimeSpan no SQL Server e MySQL, texto no SQLite.
    public static TimeOnly GetTimeOnlyValue(this DbDataReader reader, string columnName)
    {
        var value = reader[columnName];

        return value switch
        {
            TimeOnly timeOnly => timeOnly,
            TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
            DateTime dateTime => TimeOnly.FromDateTime(dateTime),
            _ => TimeOnly.Parse(value.ToString()!, CultureInfo.InvariantCulture)
        };
    }

    public static TimeOnly? GetNullableTimeOnly(this DbDataReader reader, string columnName)
    {
        return reader[columnName] is DBNull ? null : reader.GetTimeOnlyValue(columnName);
    }
}
