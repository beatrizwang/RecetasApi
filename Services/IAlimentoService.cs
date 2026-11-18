using RecetasApi.Dtos;

namespace RecetasApi.Services;

public interface IAlimentoService
{
    Task<PagedResult<AlimentoResponse>> ListarAsync(AlimentoFiltro filtro, CancellationToken ct);
    Task<AlimentoResponse?> ObtenerAsync(int id, CancellationToken ct);
    Task<AlimentoResponse> CrearAsync(AlimentoRequest request, CancellationToken ct);
    Task<AlimentoResponse?> ActualizarAsync(int id, AlimentoRequest request, CancellationToken ct);
    Task<bool> EliminarAsync(int id, CancellationToken ct);
}
