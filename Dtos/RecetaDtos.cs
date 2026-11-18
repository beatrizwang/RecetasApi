using System.ComponentModel.DataAnnotations;
using RecetasApi.Models;

namespace RecetasApi.Dtos;

public sealed class IngredienteRequest
{
    public int AlimentoId { get; set; }

    [Range(0.01, 100_000)]
    public double Cantidad { get; set; }

    public required UnidadMedida Unidad { get; set; }
}

/// <summary>Cuerpo de POST y PUT. Los ingredientes viajan anidados dentro de la receta.</summary>
public sealed class RecetaRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Descripcion { get; set; }

    [Range(1, 100)]
    public int Porciones { get; set; }

    [Range(1, 1440)]
    public int TiempoPreparacionMin { get; set; }

    public required Dificultad Dificultad { get; set; }

    [Required, MinLength(1)]
    public List<IngredienteRequest> Ingredientes { get; set; } = [];
}

public sealed class RecetaFiltro : Paginacion
{
    /// <summary>Texto contenido en el nombre de la receta.</summary>
    public string? Q { get; set; }
    public Dificultad? Dificultad { get; set; }

    /// <summary>Texto contenido en el nombre de alguno de sus ingredientes.</summary>
    public string? Alimento { get; set; }
}

public sealed record IngredienteResponse(
    int AlimentoId,
    string Alimento,
    double Cantidad,
    UnidadMedida Unidad,
    double Calorias);

/// <summary>Detalle de una receta (GET /recetas/{id}). Incluye las calorías calculadas.</summary>
public sealed record RecetaResponse(
    int Id,
    string Nombre,
    string? Descripcion,
    int Porciones,
    int TiempoPreparacionMin,
    Dificultad Dificultad,
    IReadOnlyList<IngredienteResponse> Ingredientes,
    double CaloriasTotales,
    double CaloriasPorPorcion,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);

/// <summary>Versión ligera para listados: sin ingredientes, para no cargar datos de más.</summary>
public sealed record RecetaResumenResponse(
    int Id,
    string Nombre,
    Dificultad Dificultad,
    int TiempoPreparacionMin,
    int Porciones,
    int NumeroIngredientes);
