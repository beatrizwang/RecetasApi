namespace RecetasApi.Services;

/// <summary>
/// Lógica de cálculo aislada en funciones puras: fácil de probar sin base de datos.
/// Simplificación consciente: se asume que 1 ml = 1 g (densidad 1), válido para agua/leche, no para aceite.
/// </summary>
public static class CalculoNutricional
{
    public static double CaloriasIngrediente(double caloriasPor100g, double cantidad)
        => Math.Round(caloriasPor100g * cantidad / 100d, 1);

    public static double CaloriasPorPorcion(double caloriasTotales, int porciones)
        => porciones <= 0 ? 0 : Math.Round(caloriasTotales / porciones, 1);
}
