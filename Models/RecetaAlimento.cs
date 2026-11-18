namespace RecetasApi.Models;

/// <summary>
/// Tabla intermedia Receta ↔ Alimento (muchos a muchos con datos propios: cantidad y unidad).
/// Clave compuesta (RecetaId, AlimentoId): un alimento solo puede aparecer una vez por receta.
/// </summary>
public class RecetaAlimento
{
    public int RecetaId { get; set; }
    public Receta Receta { get; set; } = null!;

    public int AlimentoId { get; set; }
    public Alimento Alimento { get; set; } = null!;

    public double Cantidad { get; set; }
    public UnidadMedida Unidad { get; set; }
}
