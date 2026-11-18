using Microsoft.AspNetCore.Mvc;
using RecetasApi.Dtos;
using RecetasApi.Services;

namespace RecetasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecetasController(IRecetaService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<RecetaResumenResponse>>> Listar([FromQuery] RecetaFiltro filtro, CancellationToken ct)
        => Ok(await service.ListarAsync(filtro, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecetaResponse>> Obtener(int id, CancellationToken ct)
    {
        var receta = await service.ObtenerAsync(id, ct);
        return receta is null ? NotFound() : Ok(receta);
    }

    [HttpPost]
    public async Task<ActionResult<RecetaResponse>> Crear(RecetaRequest request, CancellationToken ct)
    {
        var creada = await service.CrearAsync(request, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RecetaResponse>> Actualizar(int id, RecetaRequest request, CancellationToken ct)
    {
        var actualizada = await service.ActualizarAsync(id, request, ct);
        return actualizada is null ? NotFound() : Ok(actualizada);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
        => await service.EliminarAsync(id, ct) ? NoContent() : NotFound();
}
