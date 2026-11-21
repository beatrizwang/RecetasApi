using RecetasApi.Models;

namespace RecetasApi.Repositories;

public interface IAlimentoRepository
{
    Task<(IReadOnlyList<Alimento> Items, int Total)> ListarAsync(
        string? q, CategoriaAlimento? categoria, int page, int pageSize, CancellationToken ct);

    /// <summary>Lectura sin seguimiento de cambios.</summary>
    Task<Alimento?> ObtenerAsync(int id, CancellationToken ct);

    /// <summary>Con seguimiento de cambios, para modificarlo o borrarlo.</summary>
    Task<Alimento?> ObtenerParaEditarAsync(int id, CancellationToken ct);

    Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct);
    Task<bool> EstaEnUsoAsync(int id, CancellationToken ct);

    /// <summary>Cuántos de los ids indicados existen.</summary>
    Task<int> ContarExistentesAsync(IReadOnlyCollection<int> ids, CancellationToken ct);

    void Agregar(Alimento alimento);
    void Eliminar(Alimento alimento);
}
