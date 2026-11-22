using Microsoft.EntityFrameworkCore;
using RecetasApi.Data;
using RecetasApi.Interfaces;
using RecetasApi.Models;

namespace RecetasApi.Repositories;

public sealed class RecetaRepository(AppDbContext db) : IRecetaRepository
{
    public async Task<(IReadOnlyList<RecetaListadoItem> Items, int Total)> ListarAsync(
        string? q, Dificultad? dificultad, string? alimento, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Recetas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var patronNombre = $"%{q.Trim()}%";
            query = query.Where(r => EF.Functions.Like(r.Nombre, patronNombre));
        }

        if (dificultad is { } d)
        {
            query = query.Where(r => r.Dificultad == d);
        }

        if (!string.IsNullOrWhiteSpace(alimento))
        {
            var patronAlimento = $"%{alimento.Trim()}%";
            query = query.Where(r => r.Ingredientes.Any(i => EF.Functions.Like(i.Alimento.Nombre, patronAlimento)));
        }

        var total = await query.CountAsync(ct);

        // Proyección ligera: solo las columnas necesarias, sin cargar entidades completas.
        var filas = await query
            .OrderBy(r => r.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.Nombre,
                r.Dificultad,
                r.TiempoPreparacionMin,
                r.Porciones,
                Lineas = r.Ingredientes
                    .Select(i => new { i.Cantidad, i.Alimento.CaloriasPor100g })
                    .ToList()
            })
            .ToListAsync(ct);

        var items = filas
            .Select(f => new RecetaListadoItem(
                f.Id, f.Nombre, f.Dificultad, f.TiempoPreparacionMin, f.Porciones,
                f.Lineas.Select(l => new LineaNutricional(l.CaloriasPor100g, l.Cantidad)).ToList()))
            .ToList();

        return (items, total);
    }

    public Task<Receta?> ObtenerDetalleAsync(int id, CancellationToken ct)
        // Include/ThenInclude: una sola consulta con JOIN, en lugar de una consulta por ingrediente (N+1).
        => db.Recetas
            .AsNoTracking()
            .Include(r => r.Ingredientes)
                .ThenInclude(i => i.Alimento)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Receta?> ObtenerParaEditarAsync(int id, CancellationToken ct)
        => db.Recetas
            .Include(r => r.Ingredientes)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public void Agregar(Receta receta) => db.Recetas.Add(receta);

    public void Eliminar(Receta receta) => db.Recetas.Remove(receta); // Borra también sus líneas.

    public void EliminarLineas(IEnumerable<RecetaAlimento> lineas) => db.RecetaAlimentos.RemoveRange(lineas);

    public void MarcarComoModificada(Receta receta)
        => db.Entry(receta).Property(r => r.FechaActualizacion).IsModified = true;
}
