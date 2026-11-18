using System.ComponentModel.DataAnnotations;
using RecetasApi.Models;

namespace RecetasApi.Dtos;

/// <summary>Cuerpo de POST y PUT (el PUT reemplaza el recurso completo).</summary>
public sealed class AlimentoRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    public required CategoriaAlimento Categoria { get; set; }

    [Range(0, 1000)]
    public required double CaloriasPor100g { get; set; }
}

public sealed class AlimentoFiltro : Paginacion
{
    /// <summary>Texto contenido en el nombre.</summary>
    public string? Q { get; set; }
    public CategoriaAlimento? Categoria { get; set; }
}

public sealed record AlimentoResponse(
    int Id,
    string Nombre,
    CategoriaAlimento Categoria,
    double CaloriasPor100g,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);
