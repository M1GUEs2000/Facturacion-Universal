using Facturacion.Core.Entidades;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface IIdempotencyRepositorio
{
    Task<IdempotencyRecord?> ObtenerAsync(string key, Guid cuentaId, CancellationToken ct = default);
    Task GuardarAsync(IdempotencyRecord record, CancellationToken ct = default);
}
