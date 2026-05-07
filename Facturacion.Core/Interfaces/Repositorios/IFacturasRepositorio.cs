using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface IFacturasRepositorio
{
    Task<Factura?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Factura?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<IReadOnlyList<Factura>> ListarPorEmpresaAsync(string empresaRuc, EstadoSri? estado = null, CancellationToken ct = default);
    Task AgregarAsync(Factura factura, CancellationToken ct = default);
    Task ActualizarAsync(Factura factura, CancellationToken ct = default);
}
