using ErrorOr;
using Facturacion.Core.Enums;

namespace Facturacion.Core.Interfaces.Servicios;

public record MensajeSri(
    string Identificador,
    string Mensaje,
    string InformacionAdicional,
    string Tipo);

public record RespuestaRecepcionSri(
    string Estado,
    List<MensajeSri> Mensajes);

public record RespuestaAutorizacionSri(
    bool Autorizado,
    string? NumeroAutorizacion,
    DateTimeOffset? FechaAutorizacion,
    string? XmlAutorizado,
    string? MensajeSri,
    List<MensajeSri> Mensajes);

public interface IServicioSri
{
    Task<ErrorOr<RespuestaRecepcionSri>> EnviarDocumentoAsync(string xmlFirmado, Ambiente ambiente, CancellationToken ct = default);
    Task<ErrorOr<RespuestaAutorizacionSri>> ConsultarAutorizacionAsync(string claveAcceso, Ambiente ambiente, CancellationToken ct = default);
}
