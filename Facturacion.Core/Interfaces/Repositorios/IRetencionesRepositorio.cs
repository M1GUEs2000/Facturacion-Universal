using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface IRetencionesRepositorio
{
    Task<Retencion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Retencion?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<IReadOnlyList<Retencion>> ListarPorEmpresaAsync(string empresaRuc, EstadoSri? estado = null, CancellationToken ct = default);
    Task AgregarAsync(Retencion retencion, CancellationToken ct = default);
    Task ActualizarAsync(Retencion retencion, CancellationToken ct = default);
}
