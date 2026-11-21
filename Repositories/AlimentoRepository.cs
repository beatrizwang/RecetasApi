using Microsoft.EntityFrameworkCore;
using RecetasApi.Data;
using RecetasApi.Models;

namespace RecetasApi.Repositories;

public sealed class AlimentoRepository(AppDbContext db) : IAlimentoRepository
{
    public async Task<(IReadOnlyList<Alimento> Items, int Total)> ListarAsync(
        string? q, CategoriaAlimento? categoria, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Alimentos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var patron = $"%{q.Trim()}%";
            query = query.Where(a => EF.Functions.Like(a.Nombre, patron));
        }

        if (categoria is { } c)
        {
            query = query.Where(a => a.Categoria == c);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Alimento?> ObtenerAsync(int id, CancellationToken ct)
        => db.Alimentos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Alimento?> ObtenerParaEditarAsync(int id, CancellationToken ct)
        => db.Alimentos.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct)
    {
        // La columna usa colación NOCASE: "pollo" y "Pollo" cuentan como el mismo nombre.
        var query = db.Alimentos.Where(a => a.Nombre == nombre);

        if (excluirId is { } id)
        {
            query = query.Where(a => a.Id != id);
        }

        return query.AnyAsync(ct);
    }

    public Task<bool> EstaEnUsoAsync(int id, CancellationToken ct)
        => db.RecetaAlimentos.AnyAsync(ra => ra.AlimentoId == id, ct);

    public Task<int> ContarExistentesAsync(IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        var lista = ids.ToArray();
        return db.Alimentos.CountAsync(a => lista.Contains(a.Id), ct);
    }

    public void Agregar(Alimento alimento) => db.Alimentos.Add(alimento);

    public void Eliminar(Alimento alimento) => db.Alimentos.Remove(alimento);
}
