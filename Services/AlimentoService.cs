using Microsoft.EntityFrameworkCore;
using RecetasApi.Data;
using RecetasApi.Dtos;
using RecetasApi.Errors;
using RecetasApi.Models;

namespace RecetasApi.Services;

public sealed class AlimentoService(AppDbContext db) : IAlimentoService
{
    public async Task<PagedResult<AlimentoResponse>> ListarAsync(AlimentoFiltro filtro, CancellationToken ct)
    {
        // AsNoTracking: solo lectura, EF no necesita vigilar los cambios de estas entidades.
        var query = db.Alimentos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var patron = $"%{filtro.Q.Trim()}%";
            query = query.Where(a => EF.Functions.Like(a.Nombre, patron));
        }

        if (filtro.Categoria is { } categoria)
        {
            query = query.Where(a => a.Categoria == categoria);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.Nombre)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync(ct);

        return new PagedResult<AlimentoResponse>(items.Select(ToResponse).ToList(), filtro.Page, filtro.PageSize, total);
    }

    public async Task<AlimentoResponse?> ObtenerAsync(int id, CancellationToken ct)
    {
        var alimento = await db.Alimentos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        return alimento is null ? null : ToResponse(alimento);
    }

    public async Task<AlimentoResponse> CrearAsync(AlimentoRequest request, CancellationToken ct)
    {
        var nombre = request.Nombre.Trim();

        // La columna usa colación NOCASE: "pollo" y "Pollo" cuentan como el mismo nombre.
        if (await db.Alimentos.AnyAsync(a => a.Nombre == nombre, ct))
        {
            throw new ConflictException($"Ya existe un alimento llamado '{nombre}'.");
        }

        var alimento = new Alimento
        {
            Nombre = nombre,
            Categoria = request.Categoria,
            CaloriasPor100g = request.CaloriasPor100g
        };

        db.Alimentos.Add(alimento);
        await db.SaveChangesAsync(ct);

        return ToResponse(alimento);
    }

    public async Task<AlimentoResponse?> ActualizarAsync(int id, AlimentoRequest request, CancellationToken ct)
    {
        var alimento = await db.Alimentos.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (alimento is null)
        {
            return null;
        }

        var nombre = request.Nombre.Trim();

        if (await db.Alimentos.AnyAsync(a => a.Id != id && a.Nombre == nombre, ct))
        {
            throw new ConflictException($"Ya existe un alimento llamado '{nombre}'.");
        }

        alimento.Nombre = nombre;
        alimento.Categoria = request.Categoria;
        alimento.CaloriasPor100g = request.CaloriasPor100g;

        await db.SaveChangesAsync(ct);

        return ToResponse(alimento);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct)
    {
        var alimento = await db.Alimentos.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (alimento is null)
        {
            return false;
        }

        if (await db.RecetaAlimentos.AnyAsync(ra => ra.AlimentoId == id, ct))
        {
            throw new ConflictException("No se puede eliminar el alimento porque forma parte de alguna receta.");
        }

        db.Alimentos.Remove(alimento);
        await db.SaveChangesAsync(ct);

        return true;
    }

    private static AlimentoResponse ToResponse(Alimento a) =>
        new(a.Id, a.Nombre, a.Categoria, a.CaloriasPor100g, a.FechaCreacion, a.FechaActualizacion);
}
