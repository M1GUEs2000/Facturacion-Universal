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
    Func<TDoc, CancellationToken, Task> Persistir)
    where TDoc : IDocumentoEmitible;

public class OrquestadorEmision(IServicioFirma firma, IServicioSri sri, IServicioStorage storage)
{
    public async Task<ErrorOr<TDoc>> EjecutarAsync<TDoc>(
        ParametrosEmision<TDoc> p, CancellationToken ct = default)
        where TDoc : IDocumentoEmitible
    {
        var firmadoResult = await firma.FirmarXmlAsync(p.XmlSinFirmar, p.CertificadoP12, p.CertPassword, ct);
        if (firmadoResult.IsError) return firmadoResult.Errors;

        var xmlPath = $"{p.StoragePrefijo}/{p.ClaveAcceso}.xml";
        var storageXmlResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(firmadoResult.Value), xmlPath, ct);
        if (storageXmlResult.IsError) return storageXmlResult.Errors;

        p.Documento.RegistrarXmlFirmado(storageXmlResult.Value);

        var recepcionResult = await sri.EnviarDocumentoAsync(firmadoResult.Value, p.Ambiente, ct);
        if (recepcionResult.IsError) return recepcionResult.Errors;

        p.Documento.RegistrarEnvioSri();

        var autorizacionResult = await sri.ConsultarAutorizacionAsync(p.ClaveAcceso, p.Ambiente, ct);
        if (autorizacionResult.IsError) return autorizacionResult.Errors;

        var auth = autorizacionResult.Value;

        if (!auth.Autorizado)
        {
            p.Documento.RegistrarNoAutorizacion(auth.MensajeSri);
            await p.Persistir(p.Documento, ct);
            return Errores.Sri.NoAutorizado(auth.MensajeSri);
        }

        var pdfResult = await p.GenerarPdf(p.Documento, ct);
        if (pdfResult.IsError) return pdfResult.Errors;

        var pdfPath = $"{p.StoragePrefijo}/{p.ClaveAcceso}.pdf";
        var storagePdfResult = await storage.GuardarAsync(pdfResult.Value, pdfPath, ct);
        if (storagePdfResult.IsError) return storagePdfResult.Errors;

        var xmlAutPath = $"{p.StoragePrefijo}/{p.ClaveAcceso}_autorizado.xml";
        var storageXmlAutResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(auth.XmlAutorizado!), xmlAutPath, ct);
        if (storageXmlAutResult.IsError) return storageXmlAutResult.Errors;

        p.Documento.RegistrarAutorizacion(
            auth.NumeroAutorizacion!, auth.FechaAutorizacion!.Value,
            storageXmlAutResult.Value, storagePdfResult.Value);

        await p.Persistir(p.Documento, ct);

        return p.Documento;
    }
}
