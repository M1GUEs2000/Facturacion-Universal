using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface IFacturasRepositorio
{
    Task<Factura?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Factura?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteSecuencialActivoAsync(string empresaRuc, string estab, string ptoEmi, string secuencial, Ambiente ambiente, CancellationToken ct = default);
    Task<IReadOnlyList<Factura>> ListarPorEmpresaAsync(string empresaRuc, EstadoSri? estado = null, int pagina = 1, int tamanoPagina = 50, CancellationToken ct = default);
    Task<int> ContarPorEmpresaAsync(string empresaRuc, EstadoSri? estado = null, CancellationToken ct = default);
    Task AgregarAsync(Factura factura, CancellationToken ct = default);
    Task ActualizarAsync(Factura factura, CancellationToken ct = default);
}
