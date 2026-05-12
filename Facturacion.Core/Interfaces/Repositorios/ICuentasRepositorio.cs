using Facturacion.Core.Entidades;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface ICuentasRepositorio
{
    Task<Cuenta?> ObtenerPrimeraAsync(CancellationToken ct = default);
    Task<Cuenta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
}
