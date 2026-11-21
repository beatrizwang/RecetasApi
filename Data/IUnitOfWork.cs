namespace RecetasApi.Data;

/// <summary>Confirma en la base de datos los cambios pendientes de la petición.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
