using Microsoft.EntityFrameworkCore;
using RecetasApi.Data;
using RecetasApi.Dtos;
using RecetasApi.Errors;
using RecetasApi.Models;

namespace RecetasApi.Services;

public sealed class RecetaService(AppDbContext db) : IRecetaService
{
    public async Task<PagedResult<RecetaResumenResponse>> ListarAsync(RecetaFiltro filtro, CancellationToken ct)
    {
        var query = db.Recetas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var patronNombre = $"%{filtro.Q.Trim()}%";
            query = query.Where(r => EF.Functions.Like(r.Nombre, patronNombre));
        }

        if (filtro.Dificultad is { } dificultad)
        {
            query = query.Where(r => r.Dificultad == dificultad);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Alimento))
        {
            var patronAlimento = $"%{filtro.Alimento.Trim()}%";
            query = query.Where(r => r.Ingredientes.Any(i => EF.Functions.Like(i.Alimento.Nombre, patronAlimento)));
        }

        var total = await query.CountAsync(ct);

        // Proyección directa al DTO: EF solo lee las columnas necesarias y cuenta los ingredientes en SQL.
        var items = await query
            .OrderBy(r => r.Nombre)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(r => new RecetaResumenResponse(
                r.Id, r.Nombre, r.Dificultad, r.TiempoPreparacionMin, r.Porciones, r.Ingredientes.Count))
            .ToListAsync(ct);

        return new PagedResult<RecetaResumenResponse>(items, filtro.Page, filtro.PageSize, total);
    }

    public async Task<RecetaResponse?> ObtenerAsync(int id, CancellationToken ct)
    {
        // Include/ThenInclude: una sola consulta con JOIN, en lugar de una consulta por ingrediente (N+1).
        var receta = await db.Recetas
            .AsNoTracking()
            .Include(r => r.Ingredientes)
                .ThenInclude(i => i.Alimento)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return receta is null ? null : ToResponse(receta);
    }

    public async Task<RecetaResponse> CrearAsync(RecetaRequest request, CancellationToken ct)
    {
        await ValidarIngredientesAsync(request.Ingredientes, ct);

        var receta = new Receta
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            Porciones = request.Porciones,
            TiempoPreparacionMin = request.TiempoPreparacionMin,
            Dificultad = request.Dificultad,
            Ingredientes = request.Ingredientes
                .Select(i => new RecetaAlimento { AlimentoId = i.AlimentoId, Cantidad = i.Cantidad, Unidad = i.Unidad })
                .ToList()
        };

        db.Recetas.Add(receta);
        await db.SaveChangesAsync(ct);

        // Se vuelve a leer para devolver el detalle completo (con nombres de alimentos y calorías).
        return (await ObtenerAsync(receta.Id, ct))!;
    }

    public async Task<RecetaResponse?> ActualizarAsync(int id, RecetaRequest request, CancellationToken ct)
    {
        var receta = await db.Recetas
            .Include(r => r.Ingredientes)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (receta is null)
        {
            return null;
        }

        await ValidarIngredientesAsync(request.Ingredientes, ct);

        receta.Nombre = request.Nombre.Trim();
        receta.Descripcion = request.Descripcion?.Trim();
        receta.Porciones = request.Porciones;
        receta.TiempoPreparacionMin = request.TiempoPreparacionMin;
        receta.Dificultad = request.Dificultad;

        // Se sincronizan los ingredientes en lugar de borrar y recrear todo:
        // se eliminan los que ya no están, se actualizan los que siguen y se añaden los nuevos.
        var nuevos = request.Ingredientes.ToDictionary(i => i.AlimentoId);

        var obsoletos = receta.Ingredientes.Where(i => !nuevos.ContainsKey(i.AlimentoId)).ToList();
        db.RecetaAlimentos.RemoveRange(obsoletos);

        foreach (var (alimentoId, dto) in nuevos)
        {
            var existente = receta.Ingredientes.FirstOrDefault(i => i.AlimentoId == alimentoId);
            if (existente is null)
            {
                receta.Ingredientes.Add(new RecetaAlimento { AlimentoId = alimentoId, Cantidad = dto.Cantidad, Unidad = dto.Unidad });
            }
            else
            {
                existente.Cantidad = dto.Cantidad;
                existente.Unidad = dto.Unidad;
            }
        }

        // Si solo cambian los ingredientes, la receta en sí no aparece como modificada;
        // se marca para que SaveChangesAsync actualice FechaActualizacion.
        db.Entry(receta).Property(r => r.FechaActualizacion).IsModified = true;

        await db.SaveChangesAsync(ct);

        return await ObtenerAsync(id, ct);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct)
    {
        var receta = await db.Recetas
            .Include(r => r.Ingredientes)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (receta is null)
        {
            return false;
        }

        db.Recetas.Remove(receta); // Borra también sus líneas de ingredientes.
        await db.SaveChangesAsync(ct);

        return true;
    }

    private async Task ValidarIngredientesAsync(List<IngredienteRequest> ingredientes, CancellationToken ct)
    {
        var ids = ingredientes.Select(i => i.AlimentoId).ToList();

        if (ids.Distinct().Count() != ids.Count)
        {
            throw new BadRequestException("Hay alimentos repetidos en la lista de ingredientes.");
        }

        var existentes = await db.Alimentos.CountAsync(a => ids.Contains(a.Id), ct);
        if (existentes != ids.Count)
        {
            throw new BadRequestException("Alguno de los alimentos indicados no existe.");
        }
    }

    private static RecetaResponse ToResponse(Receta r)
    {
        var ingredientes = r.Ingredientes
            .OrderBy(i => i.Alimento.Nombre)
            .Select(i => new IngredienteResponse(
                i.AlimentoId,
                i.Alimento.Nombre,
                i.Cantidad,
                i.Unidad,
                CalculoNutricional.CaloriasIngrediente(i.Alimento.CaloriasPor100g, i.Cantidad)))
            .ToList();

        var total = Math.Round(ingredientes.Sum(i => i.Calorias), 1);

        return new RecetaResponse(
            r.Id,
            r.Nombre,
            r.Descripcion,
            r.Porciones,
            r.TiempoPreparacionMin,
            r.Dificultad,
            ingredientes,
            total,
            CalculoNutricional.CaloriasPorPorcion(total, r.Porciones),
            r.FechaCreacion,
            r.FechaActualizacion);
    }
}
