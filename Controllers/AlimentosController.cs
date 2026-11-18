using Microsoft.AspNetCore.Mvc;
using RecetasApi.Dtos;
using RecetasApi.Services;

namespace RecetasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlimentosController(IAlimentoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AlimentoResponse>>> Listar([FromQuery] AlimentoFiltro filtro, CancellationToken ct)
        => Ok(await service.ListarAsync(filtro, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlimentoResponse>> Obtener(int id, CancellationToken ct)
    {
        var alimento = await service.ObtenerAsync(id, ct);
        return alimento is null ? NotFound() : Ok(alimento);
    }

    [HttpPost]
    public async Task<ActionResult<AlimentoResponse>> Crear(AlimentoRequest request, CancellationToken ct)
    {
        var creado = await service.CrearAsync(request, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AlimentoResponse>> Actualizar(int id, AlimentoRequest request, CancellationToken ct)
    {
        var actualizado = await service.ActualizarAsync(id, request, ct);
        return actualizado is null ? NotFound() : Ok(actualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
        => await service.EliminarAsync(id, ct) ? NoContent() : NotFound();
}
