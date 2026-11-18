namespace RecetasApi.Models;

/// <summary>
/// Entidades con fechas de auditoría. Las rellena AppDbContext.SaveChangesAsync (siempre en UTC).
/// </summary>
public interface IAuditable
{
    DateTime FechaCreacion { get; set; }
    DateTime FechaActualizacion { get; set; }
}
