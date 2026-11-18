namespace RecetasApi.Models;

public class Receta : IAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Porciones { get; set; }
    public int TiempoPreparacionMin { get; set; }
    public Dificultad Dificultad { get; set; }

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public List<RecetaAlimento> Ingredientes { get; set; } = [];
}
