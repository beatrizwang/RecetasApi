using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RecetasApi.Data;

/// <summary>
/// SQLite devuelve los DateTime con Kind = Unspecified. Este conversor los marca como UTC
/// para que el JSON salga con "Z" y no haya ambigüedad de zona horaria.
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
