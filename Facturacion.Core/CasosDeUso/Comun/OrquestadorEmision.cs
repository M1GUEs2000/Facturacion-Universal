using System.Text;
using ErrorOr;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Servicios;

namespace Facturacion.Core.CasosDeUso.Comun;

public record ParametrosEmision<TDoc>(
    TDoc Documento,
    string ClaveAcceso,
    Ambiente Ambiente,
    string XmlSinFirmar,
    string StoragePrefijo,
    byte[] CertificadoP12,
    string CertPassword,
    Func<TDoc, CancellationToken, Task<ErrorOr<byte[]>>> GenerarPdf,
    Func<TDoc, CancellationToken, Task> Persistir,
    Func<CancellationToken, Task>? IncrementarSecuencial = null)
    where TDoc : IDocumentoEmitible;

public class OrquestadorEmision(IServicioFirma firma, IServicioSri sri, IServicioStorage storage)
{
    public async Task<ErrorOr<TDoc>> EjecutarAsync<TDoc>(
        ParametrosEmision<TDoc> p, CancellationToken ct = default)
        where TDoc : IDocumentoEmitible
    {
        // ── 1. Firma + storage XML firmado ────────────────────────────────────
        var firmadoResult = await firma.FirmarXmlAsync(p.XmlSinFirmar, p.CertificadoP12, p.CertPassword, ct);
        if (firmadoResult.IsError) return firmadoResult.Errors;

        var xmlFirmadoPath = $"{p.StoragePrefijo}/{p.ClaveAcceso}.xml";
        var storageXmlResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(firmadoResult.Value), xmlFirmadoPath, ct);
        if (storageXmlResult.IsError) return storageXmlResult.Errors;

        p.Documento.RegistrarXmlFirmado(storageXmlResult.Value);
        await p.Persistir(p.Documento, ct);

        // ── 2. Recepción SRI ──────────────────────────────────────────────────
        var recepcionResult = await sri.EnviarDocumentoAsync(firmadoResult.Value, p.Ambiente, ct);
        if (recepcionResult.IsError) 
            return recepcionResult.Errors;

        p.Documento.RegistrarEnvioSri();
        await p.Persistir(p.Documento, ct);

        if (p.IncrementarSecuencial is not null)
            await p.IncrementarSecuencial(ct);

        // ── 3. Autorización SRI ───────────────────────────────────────────────
        var autorizacionResult = await sri.ConsultarAutorizacionAsync(p.ClaveAcceso, p.Ambiente, ct);
        if (autorizacionResult.IsError) return autorizacionResult.Errors;

        var auth = autorizacionResult.Value;

        if (!auth.Autorizado)
        {
            p.Documento.RegistrarNoAutorizacion(auth.MensajeSri);
            await p.Persistir(p.Documento, ct);
            return Errores.Sri.NoAutorizado(auth.MensajeSri);
        }

        // Persiste el número de autorización antes de intentar guardar el XML.
        // Si el storage falla, la BD queda en PendienteAutorizacion + NumeroAutorizacion != null,
        // lo que permite al orquestador de reintento re-consultar el SRI y recuperar el XML.
        p.Documento.RegistrarNumeroAutorizacion(auth.NumeroAutorizacion!, auth.FechaAutorizacion!.Value, auth.MensajeSri);
        await p.Persistir(p.Documento, ct);

        // ── 4. Storage XML autorizado + borrar XML firmado ────────────────────
        var xmlAutPath = $"{p.StoragePrefijo}/{p.ClaveAcceso}_autorizado.xml";
        var storageXmlAutResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(auth.XmlAutorizado!), xmlAutPath, ct);
        if (storageXmlAutResult.IsError) return storageXmlAutResult.Errors;

        await storage.EliminarAsync(xmlFirmadoPath, ct);

        p.Documento.RegistrarAutorizacionSri(
            auth.NumeroAutorizacion!, auth.FechaAutorizacion!.Value,
            storageXmlAutResult.Value, auth.MensajeSri);
        await p.Persistir(p.Documento, ct);

        // ── 5. Generación y storage del PDF ───────────────────────────────────
        var pdfResult = await p.GenerarPdf(p.Documento, ct);
        if (pdfResult.IsError) return pdfResult.Errors;

        var pdfPath = $"{p.StoragePrefijo}/{p.ClaveAcceso}.pdf";
        var storagePdfResult = await storage.GuardarAsync(pdfResult.Value, pdfPath, ct);
        if (storagePdfResult.IsError) return storagePdfResult.Errors;

        p.Documento.RegistrarPdf(storagePdfResult.Value);
        await p.Persistir(p.Documento, ct);

        return p.Documento;
    }
}
