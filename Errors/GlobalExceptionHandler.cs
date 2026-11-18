using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RecetasApi.Errors;

/// <summary>
/// Punto único de tratamiento de errores. Devuelve siempre ProblemDetails (RFC 9457).
/// Los errores de negocio muestran su mensaje; cualquier otro se registra y devuelve un 500 genérico
/// para no filtrar detalles internos.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        if (exception is ApiException apiException)
        {
            logger.LogWarning("Error de negocio ({Status}): {Mensaje}", apiException.StatusCode, apiException.Message);
            problem = new ProblemDetails
            {
                Status = apiException.StatusCode,
                Title = apiException.Message
            };
        }
        else
        {
            logger.LogError(exception, "Error no controlado");
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Se ha producido un error interno."
            };
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });
    }
}
