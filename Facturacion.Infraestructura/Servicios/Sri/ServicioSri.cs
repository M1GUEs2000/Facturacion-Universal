using System.Text;
using System.Xml.Linq;
using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Servicios;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infraestructura.Servicios.Sri;

public class ServicioSri(IHttpClientFactory httpClientFactory, ILogger<ServicioSri> logger) : IServicioSri
{
    private const string RecepcionPruebas =
        "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl";
    private const string RecepcionProduccion =
        "https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl";
    private const string AutorizacionPruebas =
        "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline?wsdl";
    private const string AutorizacionProduccion =
        "https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline?wsdl";

    private const int MaxIntentosAutorizacion = 5;
    private static readonly TimeSpan DelayEntreIntentos = TimeSpan.FromSeconds(2);

    // ─── Recepción ────────────────────────────────────────────────────────────

    public async Task<ErrorOr<RespuestaRecepcionSri>> EnviarDocumentoAsync(
        string xmlFirmado, Ambiente ambiente, CancellationToken ct = default)
    {
        var base64Xml = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlFirmado));
        var endpoint = ambiente == Ambiente.Pruebas ? RecepcionPruebas : RecepcionProduccion;
        var soap = BuildSoapRecepcion(base64Xml);

        string responseText;
        try
        {
            responseText = await PostSoapAsync(endpoint, soap, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error HTTP enviando documento al SRI ({Endpoint})", endpoint);
            return Errores.Sri.ErrorComunicacion;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado enviando documento al SRI");
            return Errores.Sri.ErrorComunicacion;
        }

        return ParsearRecepcion(responseText);
    }

    private static ErrorOr<RespuestaRecepcionSri> ParsearRecepcion(string responseText)
    {
        XDocument doc;
        try { doc = XDocument.Parse(responseText); }
        catch (Exception) { return Errores.Sri.ErrorComunicacion; }

        var mensajes = ExtraerMensajesSri(doc);

        var nodo = doc.Descendants("RespuestaRecepcionComprobante").FirstOrDefault();
        if (nodo == null)
            return Errores.Sri.SinRespuesta;

        var estado = nodo.Element("estado")?.Value ?? "";

        return estado switch
        {
            "RECIBIDA" => new RespuestaRecepcionSri(estado, mensajes),
            "CLAVE ACCESO REGISTRADA" => Errores.Sri.SecuencialDuplicado,
            "EN PROCESAMIENTO" => Errores.Sri.EnProcesamiento,
            _ => Errores.Sri.Devuelta(FormatearMensajes(mensajes))
        };
    }

    // ─── Autorización ────────────────────────────────────────────────────────

    public async Task<ErrorOr<RespuestaAutorizacionSri>> ConsultarAutorizacionAsync(
        string claveAcceso, Ambiente ambiente, CancellationToken ct = default)
    {
        var endpoint = ambiente == Ambiente.Pruebas ? AutorizacionPruebas : AutorizacionProduccion;
        var soap = BuildSoapAutorizacion(claveAcceso);

        ErrorOr<RespuestaAutorizacionSri> ultimaRespuesta = Errores.Sri.SinRespuesta;

        for (int intento = 1; intento <= MaxIntentosAutorizacion; intento++)
        {
            string responseText;
            try
            {
                responseText = await PostSoapAsync(endpoint, soap, ct);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Error HTTP consultando autorizacion SRI ({Endpoint})", endpoint);
                return Errores.Sri.ErrorComunicacion;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado consultando autorizacion SRI");
                return Errores.Sri.ErrorComunicacion;
            }

            ultimaRespuesta = ParsearAutorizacion(responseText);

            if (ultimaRespuesta.IsError)
            {
                // Solo reintenta si el error es EnProcesamiento
                if (ultimaRespuesta.FirstError.Code == "Sri.EnProcesamiento"
                    && intento < MaxIntentosAutorizacion)
                {
                    await Task.Delay(DelayEntreIntentos, ct);
                    continue;
                }
                return ultimaRespuesta;
            }

            return ultimaRespuesta;
        }

        return ultimaRespuesta;
    }

    private static ErrorOr<RespuestaAutorizacionSri> ParsearAutorizacion(string responseText)
    {
        XDocument doc;
        try { doc = XDocument.Parse(responseText); }
        catch (Exception) { return Errores.Sri.ErrorComunicacion; }

        var mensajesGlobales = ExtraerMensajesSri(doc);

        var root = doc.Descendants("RespuestaAutorizacionComprobante").FirstOrDefault();
        if (root == null)
            return Errores.Sri.SinRespuesta;

        var auth = root.Descendants("autorizacion").FirstOrDefault();
        if (auth == null)
        {
            // Puede ser EN PROCESAMIENTO sin nodo autorizacion aún
            return Errores.Sri.EnProcesamiento;
        }

        var estado = auth.Element("estado")?.Value ?? "";
        var mensajesAuth = auth.Element("mensajes")?
            .Elements("mensaje")
            .Select(m => new MensajeSri(
                m.Element("identificador")?.Value ?? "",
                m.Element("mensaje")?.Value ?? "",
                m.Element("informacionAdicional")?.Value ?? "",
                m.Element("tipo")?.Value ?? ""))
            .ToList() ?? [];

        // Combinar mensajes del nodo auth con los globales del SOAP
        var todosMensajes = mensajesAuth.Concat(mensajesGlobales).ToList();

        return estado switch
        {
            "AUTORIZADO"      => ParsearAutorizado(auth, todosMensajes),
            // NO AUTORIZADO se devuelve como valor (Autorizado=false) para que el caso de uso
            // pueda persistir el documento con estado NoAutorizado antes de retornar el error.
            "NO AUTORIZADO"   => new RespuestaAutorizacionSri(false, null, null, null, FormatearMensajes(todosMensajes), todosMensajes),
            "EN PROCESAMIENTO" => Errores.Sri.EnProcesamiento,
            _                 => Errores.Sri.SinRespuesta
        };
    }

    private static ErrorOr<RespuestaAutorizacionSri> ParsearAutorizado(
        XElement auth, List<MensajeSri> mensajes)
    {
        var numeroAut = auth.Element("numeroAutorizacion")?.Value;
        var fechaStr = auth.Element("fechaAutorizacion")?.Value;
        var xmlAutorizado = auth.Element("comprobante")?.Value
                         ?? auth.Element("comprobanteRetencion")?.Value;

        DateTimeOffset? fechaAut = null;
        if (!string.IsNullOrEmpty(fechaStr) && DateTimeOffset.TryParse(fechaStr, out var fecha))
            fechaAut = fecha;

        var resumen = FormatearMensajes(mensajes);
        return new RespuestaAutorizacionSri(true, numeroAut, fechaAut, xmlAutorizado, resumen, mensajes);
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

    // ─── Helpers ──────────────────────────────────────────────────────────────

    // Port de CSriws.ExtraerMensajesSri — extrae mensajes estructurados del SRI
    // de cualquier parte del SOAP (recepción o autorización).
    private static List<MensajeSri> ExtraerMensajesSri(XDocument doc)
    {
        var resultado = new List<MensajeSri>();

        foreach (var nodo in doc.Descendants("mensaje"))
        {
            // Solo procesar nodos que tengan al menos un campo SRI estructurado
            bool esMensajeSri =
                nodo.Element("identificador") != null ||
                nodo.Element("mensaje") != null ||
                nodo.Element("informacionAdicional") != null ||
                nodo.Element("tipo") != null;

            if (!esMensajeSri) continue;

            resultado.Add(new MensajeSri(
                nodo.Element("identificador")?.Value ?? "",
                nodo.Element("mensaje")?.Value ?? "",
                nodo.Element("informacionAdicional")?.Value ?? "",
                nodo.Element("tipo")?.Value ?? ""));
        }

        return resultado;
    }

    private static string? FormatearMensajes(List<MensajeSri> mensajes)
    {
        if (mensajes.Count == 0) return null;

        return string.Join(" | ", mensajes.Select(m =>
            string.IsNullOrEmpty(m.InformacionAdicional)
                ? m.Mensaje
                : $"{m.Mensaje}: {m.InformacionAdicional}"));
    }
}
