using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Comun;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface INotasCreditoRepositorio
{
    Task<NotaCredito?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<NotaCredito?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default);
    Task<bool> ExisteSecuencialActivoAsync(string empresaRuc, string estab, string ptoEmi, string secuencial, Ambiente ambiente, CancellationToken ct = default);
    Task<IReadOnlyList<NotaCredito>> ListarConCursorAsync(string empresaRuc, EstadoSri? estado, CursorDePagina? cursor, int tamanoPagina, CancellationToken ct = default);
    Task AgregarAsync(NotaCredito notaCredito, CancellationToken ct = default);
    Task ActualizarAsync(NotaCredito notaCredito, CancellationToken ct = default);
}
