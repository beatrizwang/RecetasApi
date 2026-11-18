using RecetasApi.Models;

namespace RecetasApi.Data;

/// <summary>
/// Datos iniciales para poder probar la API sin crear alimentos a mano.
/// HasData exige valores fijos (no DateTime.UtcNow), por eso la fecha es constante.
/// </summary>
public static class SeedData
{
    private static readonly DateTime Fecha = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Alimento[] Alimentos =>
    [
        new() { Id = 1, Nombre = "Pechuga de pollo", Categoria = CategoriaAlimento.Carne,   CaloriasPor100g = 165, FechaCreacion = Fecha, FechaActualizacion = Fecha },
        new() { Id = 2, Nombre = "Arroz",            Categoria = CategoriaAlimento.Cereal,  CaloriasPor100g = 360, FechaCreacion = Fecha, FechaActualizacion = Fecha },
        new() { Id = 3, Nombre = "Tomate",           Categoria = CategoriaAlimento.Verdura, CaloriasPor100g = 18,  FechaCreacion = Fecha, FechaActualizacion = Fecha },
        new() { Id = 4, Nombre = "Aceite de oliva",  Categoria = CategoriaAlimento.Grasa,   CaloriasPor100g = 884, FechaCreacion = Fecha, FechaActualizacion = Fecha },
        new() { Id = 5, Nombre = "Huevo",            Categoria = CategoriaAlimento.Otro,    CaloriasPor100g = 143, FechaCreacion = Fecha, FechaActualizacion = Fecha },
        new() { Id = 6, Nombre = "Leche entera",     Categoria = CategoriaAlimento.Lacteo,  CaloriasPor100g = 61,  FechaCreacion = Fecha, FechaActualizacion = Fecha },
    ];
}
