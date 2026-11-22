using RecetasApi.Data;
using RecetasApi.Dtos;
using RecetasApi.Errors;
using RecetasApi.Interfaces;
using RecetasApi.Models;

namespace RecetasApi.Services;

public sealed class RecetaService(
    IRecetaRepository recetas,
    IAlimentoRepository alimentos,
    IUnitOfWork unitOfWork) : IRecetaService
{
    public async Task<PagedResult<RecetaResumenResponse>> ListarAsync(RecetaFiltro filtro, CancellationToken ct)
    {
        var (filas, totalRegistros) = await recetas.ListarAsync(
            filtro.Q, filtro.Dificultad, filtro.Alimento, filtro.Page, filtro.PageSize, ct);

        // Las calorías se calculan con la misma función que usa el detalle,
        // así el listado y el detalle de una receta siempre muestran el mismo número.
        var items = filas.Select(f =>
        {
            return new RecetaResumenResponse(
                f.Id, f.Nombre, f.Dificultad, f.TiempoPreparacionMin, f.Porciones, f.Lineas.Count);
        }).ToList();

        return new PagedResult<RecetaResumenResponse>(items, filtro.Page, filtro.PageSize, totalRegistros);
    }

    public async Task<RecetaResponse?> ObtenerAsync(int id, CancellationToken ct)
    {
        var receta = await recetas.ObtenerDetalleAsync(id, ct);
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

        recetas.Agregar(receta);
        await unitOfWork.SaveChangesAsync(ct);

        // Se vuelve a leer para devolver el detalle completo (con nombres de alimentos y calorías).
        return (await ObtenerAsync(receta.Id, ct))!;
    }

    public async Task<RecetaResponse?> ActualizarAsync(int id, RecetaRequest request, CancellationToken ct)
    {
        var receta = await recetas.ObtenerParaEditarAsync(id, ct);
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
        recetas.EliminarLineas(obsoletos);

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
        recetas.MarcarComoModificada(receta);

        await unitOfWork.SaveChangesAsync(ct);

        return await ObtenerAsync(id, ct);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct)
    {
        var receta = await recetas.ObtenerParaEditarAsync(id, ct);
        if (receta is null)
        {
            return false;
        }

        recetas.Eliminar(receta); // Borra también sus líneas de ingredientes.
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private async Task ValidarIngredientesAsync(List<IngredienteRequest> ingredientes, CancellationToken ct)
    {
        var ids = ingredientes.Select(i => i.AlimentoId).ToList();

        if (ids.Distinct().Count() != ids.Count)
        {
            throw new BadRequestException("Hay alimentos repetidos en la lista de ingredientes.");
        }

        if (await alimentos.ContarExistentesAsync(ids, ct) != ids.Count)
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