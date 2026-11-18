namespace RecetasApi.Errors;

/// <summary>Errores "esperados" de negocio. El GlobalExceptionHandler los traduce a un código HTTP.</summary>
public abstract class ApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

/// <summary>409: la operación choca con el estado actual (duplicado, recurso en uso...).</summary>
public sealed class ConflictException(string message) : ApiException(message, StatusCodes.Status409Conflict);

/// <summary>400: los datos son válidos de forma pero incorrectos de fondo (p. ej. un alimento que no existe).</summary>
public sealed class BadRequestException(string message) : ApiException(message, StatusCodes.Status400BadRequest);
