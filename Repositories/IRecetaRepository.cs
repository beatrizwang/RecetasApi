using RecetasApi.Models;

namespace RecetasApi.Repositories;

public interface IRecetaRepository
{
    Task<(IReadOnlyList<RecetaListadoItem> Items, int Total)> ListarAsync(
        string? q, Dificultad? dificultad, string? alimento, int page, int pageSize, CancellationToken ct);

    /// <summary>Receta con ingredientes y alimentos, sin seguimiento de cambios.</summary>
    Task<Receta?> ObtenerDetalleAsync(int id, CancellationToken ct);

    /// <summary>Receta con sus ingredientes y con seguimiento de cambios, para modificarla o borrarla.</summary>
    Task<Receta?> ObtenerParaEditarAsync(int id, CancellationToken ct);

    void Agregar(Receta receta);
    void Eliminar(Receta receta);
    void EliminarLineas(IEnumerable<RecetaAlimento> lineas);

    /// <summary>Fuerza la actualización de FechaActualizacion aunque solo hayan cambiado los ingredientes.</summary>
    void MarcarComoModificada(Receta receta);
}
