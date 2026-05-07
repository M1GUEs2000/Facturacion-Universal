using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Servicios;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Facturacion.Infraestructura.Servicios.Pdf;

public class ServicioPdf : IServicioPdf
{
    static ServicioPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<ErrorOr<byte[]>> GenerarRideFacturaAsync(Factura factura, CancellationToken ct = default)
    {
        var bytes = Document.Create(c => c.Page(p =>
        {
            p.Content().Text($"RIDE Factura {factura.ClaveAcceso}");
        })).GeneratePdf();

        return Task.FromResult<ErrorOr<byte[]>>(bytes);
    }

    public Task<ErrorOr<byte[]>> GenerarRideNotaCreditoAsync(NotaCredito notaCredito, CancellationToken ct = default)
    {
        var bytes = Document.Create(c => c.Page(p =>
        {
            p.Content().Text($"RIDE Nota de Crédito {notaCredito.ClaveAcceso}");
        })).GeneratePdf();

        return Task.FromResult<ErrorOr<byte[]>>(bytes);
    }

    public Task<ErrorOr<byte[]>> GenerarRideRetencionAsync(Retencion retencion, CancellationToken ct = default)
    {
        var bytes = Document.Create(c => c.Page(p =>
        {
            p.Content().Text($"RIDE Retención {retencion.ClaveAcceso}");
        })).GeneratePdf();

        return Task.FromResult<ErrorOr<byte[]>>(bytes);
    }
}
