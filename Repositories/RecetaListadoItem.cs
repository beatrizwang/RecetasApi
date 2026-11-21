using RecetasApi.Models;

namespace RecetasApi.Repositories;

/// <summary>Una línea de ingrediente reducida a lo necesario para calcular calorías.</summary>
public sealed record LineaNutricional(double CaloriasPor100g, double Cantidad);

/// <summary>Fila del listado de recetas: datos básicos y líneas para calcular las calorías.</summary>
public sealed record RecetaListadoItem(
    int Id,
    string Nombre,
    Dificultad Dificultad,
    int TiempoPreparacionMin,
    int Porciones,
    IReadOnlyList<LineaNutricional> Lineas);
