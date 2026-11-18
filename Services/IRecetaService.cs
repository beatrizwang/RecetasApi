using RecetasApi.Dtos;

namespace RecetasApi.Services;

public interface IRecetaService
{
    Task<PagedResult<RecetaResumenResponse>> ListarAsync(RecetaFiltro filtro, CancellationToken ct);
    Task<RecetaResponse?> ObtenerAsync(int id, CancellationToken ct);
    Task<RecetaResponse> CrearAsync(RecetaRequest request, CancellationToken ct);
    Task<RecetaResponse?> ActualizarAsync(int id, RecetaRequest request, CancellationToken ct);
    Task<bool> EliminarAsync(int id, CancellationToken ct);
}
