using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Comun;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface IRetencionesRepositorio
{
    Task<Retencion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Retencion?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteSecuencialActivoAsync(string empresaRuc, string estab, string ptoEmi, string secuencial, Ambiente ambiente, CancellationToken ct = default);
    Task<IReadOnlyList<Retencion>> ListarConCursorAsync(string empresaRuc, EstadoSri? estado, CursorDePagina? cursor, int tamanoPagina, CancellationToken ct = default);
    Task AgregarAsync(Retencion retencion, CancellationToken ct = default);
    Task ActualizarAsync(Retencion retencion, CancellationToken ct = default);
}
