using ErrorOr;
using Facturacion.Core.Entidades;

namespace Facturacion.Core.Interfaces.Servicios;

public interface IServicioPdf
{
    Task<ErrorOr<byte[]>> GenerarRideFacturaAsync(Factura factura, CancellationToken ct = default);
    Task<ErrorOr<byte[]>> GenerarRideNotaCreditoAsync(NotaCredito notaCredito, CancellationToken ct = default);
    Task<ErrorOr<byte[]>> GenerarRideRetencionAsync(Retencion retencion, CancellationToken ct = default);
}
