using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface IRetencionesRepositorio
{
    Task<Retencion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Retencion?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteSecuencialActivoAsync(string empresaRuc, string estab, string ptoEmi, string secuencial, Ambiente ambiente, CancellationToken ct = default);
    Task<IReadOnlyList<Retencion>> ListarPorEmpresaAsync(string empresaRuc, EstadoSri? estado = null, int pagina = 1, int tamanoPagina = 50, CancellationToken ct = default);
    Task<int> ContarPorEmpresaAsync(string empresaRuc, EstadoSri? estado = null, CancellationToken ct = default);
    Task AgregarAsync(Retencion retencion, CancellationToken ct = default);
    Task ActualizarAsync(Retencion retencion, CancellationToken ct = default);
}
