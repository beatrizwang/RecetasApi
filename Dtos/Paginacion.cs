using System.ComponentModel.DataAnnotations;

namespace RecetasApi.Dtos;

/// <summary>Parámetros de paginación comunes a los listados (?page=1&amp;pageSize=20).</summary>
public class Paginacion
{
    [Range(1, 10_000)]
    public int Page { get; set; } = 1;

    // Tope máximo para que nadie pida "todo" de golpe.
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
