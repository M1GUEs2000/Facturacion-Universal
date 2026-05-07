using ErrorOr;

namespace Facturacion.Core.Interfaces.Servicios;

public interface IServicioFirma
{
    Task<ErrorOr<string>> FirmarXmlAsync(string xml, byte[] certificadoP12, string password, CancellationToken ct = default);
}
