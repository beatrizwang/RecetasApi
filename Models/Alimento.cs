namespace RecetasApi.Models;

public class Alimento : IAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public CategoriaAlimento Categoria { get; set; }
    public double CaloriasPor100g { get; set; }

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public List<RecetaAlimento> Recetas { get; set; } = [];
}
