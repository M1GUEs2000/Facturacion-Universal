using System.Text;
using System.Xml.Linq;
using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Servicios;

namespace Facturacion.Infraestructura.Servicios.Sri;

public class ServicioSri(IHttpClientFactory httpClientFactory) : IServicioSri
{
    private const string RecepcionPruebas =
        "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl";
    private const string RecepcionProduccion =
        "https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl";
    private const string AutorizacionPruebas =
        "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline?wsdl";
    private const string AutorizacionProduccion =
        "https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline?wsdl";

    public async Task<ErrorOr<bool>> EnviarDocumentoAsync(
        string xmlFirmado, Ambiente ambiente, CancellationToken ct = default)
    {
        var base64Xml = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlFirmado));
        var endpoint = ambiente == Ambiente.Pruebas ? RecepcionPruebas : RecepcionProduccion;
        var soap = BuildSoapRecepcion(base64Xml);

        try
        {
            var responseText = await PostSoapAsync(endpoint, soap, ct);
            var doc = XDocument.Parse(responseText);
            var respuesta = doc.Descendants("RespuestaRecepcionComprobante").FirstOrDefault();
            if (respuesta == null)
                return Errores.Sri.ErrorComunicacion;

            var estado = respuesta.Element("estado")?.Value ?? "";
            return estado == "RECIBIDA" ? true : Errores.Sri.Rechazado;
        }
        catch (HttpRequestException)
        {
            return Errores.Sri.ErrorComunicacion;
        }
        catch (Exception)
        {
            return Errores.Sri.ErrorComunicacion;
        }
    }

    public async Task<ErrorOr<RespuestaAutorizacionSri>> ConsultarAutorizacionAsync(
        string claveAcceso, Ambiente ambiente, CancellationToken ct = default)
    {
        var endpoint = ambiente == Ambiente.Pruebas ? AutorizacionPruebas : AutorizacionProduccion;
        var soap = BuildSoapAutorizacion(claveAcceso);

        try
        {
            var responseText = await PostSoapAsync(endpoint, soap, ct);
            return ParseRespuestaAutorizacion(responseText);
        }
        catch (HttpRequestException)
        {
            return Errores.Sri.ErrorComunicacion;
        }
        catch (Exception)
        {
            return Errores.Sri.ErrorComunicacion;
        }
    }

    // ─── SOAP builders ────────────────────────────────────────────────────────

    private static string BuildSoapRecepcion(string base64Xml) =>
        $"""
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ec="http://ec.gob.sri.ws.recepcion">
            <soapenv:Header/>
            <soapenv:Body>
                <ec:validarComprobante>
                    <xml>{base64Xml}</xml>
                </ec:validarComprobante>
            </soapenv:Body>
        </soapenv:Envelope>
        """;

    private static string BuildSoapAutorizacion(string claveAcceso) =>
        $"""
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ec="http://ec.gob.sri.ws.autorizacion">
            <soapenv:Header/>
            <soapenv:Body>
                <ec:autorizacionComprobante>
                    <claveAccesoComprobante>{claveAcceso}</claveAccesoComprobante>
                </ec:autorizacionComprobante>
            </soapenv:Body>
        </soapenv:Envelope>
        """;

    // ─── HTTP ─────────────────────────────────────────────────────────────────

    private async Task<string> PostSoapAsync(string endpoint, string soap, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("sri");
        using var content = new StringContent(soap, Encoding.UTF8, "text/xml");
        var response = await client.PostAsync(endpoint, content, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    // ─── Response parsing ─────────────────────────────────────────────────────

    private static ErrorOr<RespuestaAutorizacionSri> ParseRespuestaAutorizacion(string responseText)
    {
        var doc = XDocument.Parse(responseText);
        var root = doc.Descendants("RespuestaAutorizacionComprobante").FirstOrDefault();
        if (root == null)
            return Errores.Sri.ErrorComunicacion;

        var auth = root.Descendants("autorizacion").FirstOrDefault();
        if (auth == null)
            return new RespuestaAutorizacionSri(false, null, null, null, "Sin autorizaciones disponibles");

        var estado = auth.Element("estado")?.Value ?? "";
        var numeroAut = auth.Element("numeroAutorizacion")?.Value;
        var fechaStr = auth.Element("fechaAutorizacion")?.Value;
        var xmlAutorizado = auth.Element("comprobante")?.Value
                         ?? auth.Element("comprobanteRetencion")?.Value;

        var mensajes = auth.Element("mensajes")?
            .Elements("mensaje")
            .Select(m => m.Element("mensaje")?.Value)
            .Where(m => !string.IsNullOrEmpty(m))
            .ToList() ?? [];
        var mensajeSri = mensajes.Count > 0 ? string.Join(" | ", mensajes) : null;

        DateTimeOffset? fechaAut = null;
        if (!string.IsNullOrEmpty(fechaStr) && DateTimeOffset.TryParse(fechaStr, out var fecha))
            fechaAut = fecha;

        if (estado != "AUTORIZADO")
            return Errores.Sri.NoAutorizado(mensajeSri);

        return new RespuestaAutorizacionSri(true, numeroAut, fechaAut, xmlAutorizado, mensajeSri);
    }
}
