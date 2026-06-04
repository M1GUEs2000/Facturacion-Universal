using Facturacion.Core.Entidades;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface ICuentasRepositorio
{
    Task<Cuenta?> ObtenerPrimeraAsync(CancellationToken ct = default);
    Task<Cuenta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<List<string>> ObtenerRucsPorCuentaAsync(Guid cuentaId, CancellationToken ct = default);
    Task<List<(string? XmlFirmado, string? XmlAutorizado, string? Pdf)>> ObtenerPathsDocumentosPorRucsAsync(
        IReadOnlyList<string> rucs, CancellationToken ct = default);
    Task EliminarCuentaAsync(Guid cuentaId, IReadOnlyList<string> rucs, CancellationToken ct = default);
}
