using System.Text;
using ErrorOr;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;
using Microsoft.Extensions.Logging;

namespace Facturacion.Core.CasosDeUso.Comun;

public record ParametrosReintento<TDoc>(
    TDoc Documento,
    string ClaveAcceso,
    Ambiente Ambiente,
    string StoragePrefijo,
    byte[] CertificadoP12,
    string CertPassword,
    Func<TDoc, CancellationToken, ErrorOr<string>> GenerarXmlSinFirmar,
    Func<TDoc, CancellationToken, Task<ErrorOr<byte[]>>> GenerarPdf,
    Func<TDoc, CancellationToken, Task> Persistir)
    where TDoc : IDocumentoEmitible;

public class OrquestadorReintento(
    IServicioFirma firma, IServicioSri sri, IServicioStorage storage,
    ILogger<OrquestadorReintento> logger)
{
    public async Task<ErrorOr<TDoc>> EjecutarAsync<TDoc>(
        ParametrosReintento<TDoc> p, CancellationToken ct = default)
        where TDoc : IDocumentoEmitible
    {
        var doc = p.Documento;

        // ── Paso 1: Firma + storage XML firmado ───────────────────────────────
        // Solo si no hay XML firmado guardado todavía.
        if (doc.XmlFirmadoPath is null)
        {
            var xmlResult = p.GenerarXmlSinFirmar(doc, ct);
            if (xmlResult.IsError) return xmlResult.Errors;

            var firmadoResult = await firma.FirmarXmlAsync(xmlResult.Value, p.CertificadoP12, p.CertPassword, ct);
            if (firmadoResult.IsError) return firmadoResult.Errors;

            var xmlFirmadoPath = RutasStorage.XmlFirmado(p.StoragePrefijo, p.ClaveAcceso);
            var storageResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(firmadoResult.Value), xmlFirmadoPath, ct);
            if (storageResult.IsError) return storageResult.Errors;

            doc.RegistrarXmlFirmado(storageResult.Value);
            await p.Persistir(doc, ct);
        }

        // ── Paso 2: Recepción SRI ─────────────────────────────────────────────
        // Solo si aún no está en PendienteAutorizacion o posterior.
        if (doc.EstadoSri is EstadoSri.Pendiente or EstadoSri.Enviado)
        {
            var xmlBytesResult = await storage.ObtenerAsync(doc.XmlFirmadoPath!, ct);
            if (xmlBytesResult.IsError) return xmlBytesResult.Errors;

            var xmlFirmado = Encoding.UTF8.GetString(xmlBytesResult.Value);

            var recepcionResult = await sri.EnviarDocumentoAsync(xmlFirmado, p.Ambiente, ct);

            // CLAVE ACCESO REGISTRADA = ya fue recibido previamente, continuar.
            if (recepcionResult.IsError && recepcionResult.FirstError.Code != "Sri.SecuencialDuplicado")
                return recepcionResult.Errors;

            doc.RegistrarEnvioSri();
            await p.Persistir(doc, ct);
        }

        // ── Paso 3: Autorización SRI + storage XML autorizado ─────────────────
        // Si NumeroAutorizacion es null → aún no fue autorizado (o fallo storage).
        // Si NumeroAutorizacion != null pero XmlAutorizadoPath es null → storage del XML falló,
        // re-consultamos el SRI para recuperar el XML autorizado.
        if (doc.XmlAutorizadoPath is null)
        {
            var autorizacionResult = await sri.ConsultarAutorizacionAsync(p.ClaveAcceso, p.Ambiente, ct);
            if (autorizacionResult.IsError) return autorizacionResult.Errors;

            var auth = autorizacionResult.Value;

            if (!auth.Autorizado)
            {
                if (doc.XmlFirmadoPath is not null)
                {
                    var borrarResult = await storage.EliminarAsync(doc.XmlFirmadoPath, ct);
                    if (borrarResult.IsError)
                        logger.LogWarning(
                            "No se pudo eliminar XML firmado {Path} tras rechazo SRI de {ClaveAcceso}: {Error}",
                            doc.XmlFirmadoPath, p.ClaveAcceso, borrarResult.FirstError.Description);
                }
                doc.RegistrarNoAutorizacion(auth.MensajeSri);
                await p.Persistir(doc, ct);
                return Errores.Sri.NoAutorizado(auth.MensajeSri);
            }

            // Persiste el número antes de intentar guardar el XML, por si el storage falla.
            doc.RegistrarNumeroAutorizacion(auth.NumeroAutorizacion!, auth.FechaAutorizacion!.Value, auth.MensajeSri);
            await p.Persistir(doc, ct);

            var xmlAutPath = RutasStorage.XmlAutorizado(p.StoragePrefijo, p.ClaveAcceso);
            var storageXmlAutResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(auth.XmlAutorizado!), xmlAutPath, ct);
            if (storageXmlAutResult.IsError) return storageXmlAutResult.Errors;

            // Borra el XML firmado si todavía existe.
            if (doc.XmlFirmadoPath is not null)
            {
                var borrarResult = await storage.EliminarAsync(doc.XmlFirmadoPath, ct);
                if (borrarResult.IsError)
                    logger.LogWarning(
                        "No se pudo eliminar XML firmado {Path} tras autorización de {ClaveAcceso}: {Error}",
                        doc.XmlFirmadoPath, p.ClaveAcceso, borrarResult.FirstError.Description);
            }

            doc.RegistrarAutorizacionSri(
                auth.NumeroAutorizacion!, auth.FechaAutorizacion!.Value,
                storageXmlAutResult.Value, auth.MensajeSri);
            await p.Persistir(doc, ct);
        }

        // ── Paso 4: PDF ───────────────────────────────────────────────────────
        if (doc.PdfPath is null)
        {
            var pdfResult = await p.GenerarPdf(doc, ct);
            if (pdfResult.IsError) return pdfResult.Errors;

            var pdfPath = RutasStorage.Pdf(p.StoragePrefijo, p.ClaveAcceso);
            var storagePdfResult = await storage.GuardarAsync(pdfResult.Value, pdfPath, ct);
            if (storagePdfResult.IsError) return storagePdfResult.Errors;

            doc.RegistrarPdf(storagePdfResult.Value);
            await p.Persistir(doc, ct);
        }

        return doc;
    }
}
