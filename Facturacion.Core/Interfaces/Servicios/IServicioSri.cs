using ErrorOr;
using Facturacion.Core.Enums;

namespace Facturacion.Core.Interfaces.Servicios;

public record RespuestaAutorizacionSri(
    bool Autorizado,
    string? NumeroAutorizacion,
    DateTimeOffset? FechaAutorizacion,
    string? XmlAutorizado,
    string? MensajeSri);

public interface IServicioSri
{
    Task<ErrorOr<bool>> EnviarDocumentoAsync(string xmlFirmado, Ambiente ambiente, CancellationToken ct = default);
    Task<ErrorOr<RespuestaAutorizacionSri>> ConsultarAutorizacionAsync(string claveAcceso, Ambiente ambiente, CancellationToken ct = default);
}
