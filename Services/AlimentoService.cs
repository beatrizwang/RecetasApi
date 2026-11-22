using RecetasApi.Data;
using RecetasApi.Dtos;
using RecetasApi.Errors;
using RecetasApi.Models;
using RecetasApi.Interfaces;

namespace RecetasApi.Services;

public sealed class AlimentoService(IAlimentoRepository alimentos, IUnitOfWork unitOfWork) : IAlimentoService
{
    public async Task<PagedResult<AlimentoResponse>> ListarAsync(AlimentoFiltro filtro, CancellationToken ct)
    {
        var (items, total) = await alimentos.ListarAsync(filtro.Q, filtro.Categoria, filtro.Page, filtro.PageSize, ct);

        return new PagedResult<AlimentoResponse>(items.Select(ToResponse).ToList(), filtro.Page, filtro.PageSize, total);
    }

    public async Task<AlimentoResponse?> ObtenerAsync(int id, CancellationToken ct)
    {
        var alimento = await alimentos.ObtenerAsync(id, ct);
        return alimento is null ? null : ToResponse(alimento);
    }

    public async Task<AlimentoResponse> CrearAsync(AlimentoRequest request, CancellationToken ct)
    {
        var nombre = request.Nombre.Trim();

        if (await alimentos.ExisteNombreAsync(nombre, null, ct))
        {
            throw new ConflictException($"Ya existe un alimento llamado '{nombre}'.");
        }

        var alimento = new Alimento
        {
            Nombre = nombre,
            Categoria = request.Categoria,
            CaloriasPor100g = request.CaloriasPor100g
        };

        alimentos.Agregar(alimento);
        await unitOfWork.SaveChangesAsync(ct);

        return ToResponse(alimento);
    }

    public async Task<AlimentoResponse?> ActualizarAsync(int id, AlimentoRequest request, CancellationToken ct)
    {
        var alimento = await alimentos.ObtenerParaEditarAsync(id, ct);
        if (alimento is null)
        {
            return null;
        }

        var nombre = request.Nombre.Trim();

        if (await alimentos.ExisteNombreAsync(nombre, id, ct))
        {
            throw new ConflictException($"Ya existe un alimento llamado '{nombre}'.");
        }

        alimento.Nombre = nombre;
        alimento.Categoria = request.Categoria;
        alimento.CaloriasPor100g = request.CaloriasPor100g;

        await unitOfWork.SaveChangesAsync(ct);

        return ToResponse(alimento);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct)
    {
        var alimento = await alimentos.ObtenerParaEditarAsync(id, ct);
        if (alimento is null)
        {
            return false;
        }

        if (await alimentos.EstaEnUsoAsync(id, ct))
        {
            throw new ConflictException("No se puede eliminar el alimento porque forma parte de alguna receta.");
        }

        alimentos.Eliminar(alimento);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private static AlimentoResponse ToResponse(Alimento a) =>
        new(a.Id, a.Nombre, a.Categoria, a.CaloriasPor100g, a.FechaCreacion, a.FechaActualizacion);
}
