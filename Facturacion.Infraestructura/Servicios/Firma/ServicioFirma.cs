using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Servicios;
using FirmaXadesNetCore;
using FirmaXadesNetCore.Crypto;
using FirmaXadesNetCore.Signature.Parameters;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infraestructura.Servicios.Firma;

public class ServicioFirma(ILogger<ServicioFirma> logger) : IServicioFirma
{
    public Task<ErrorOr<string>> FirmarXmlAsync(
        string xml, byte[] certificadoP12, string password, CancellationToken ct = default)
        => Task.Run(() => Firmar(xml, certificadoP12, password, logger), ct);

    private static ErrorOr<string> Firmar(string xml, byte[] certificadoP12, string password, ILogger logger)
    {
        X509Certificate2 cert;
        try
        {
            cert = new X509Certificate2(
                certificadoP12,
                password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (CryptographicException ex)
        {
            logger.LogError(ex, "Certificado P12 invalido o password incorrecto");
            return Errores.Firma.CertificadoInvalido;
        }

        try
        {
            using var signer = new Signer(cert);
            var parametros = new SignatureParameters
            {
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                SignatureMethod = SignatureMethod.RSAwithSHA256,
                DigestMethod = DigestMethod.SHA256,
                Signer = signer
            };

            var service = new XadesService();
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var docFirmado = service.Sign(ms, parametros);
            return docFirmado.Document.OuterXml;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error firmando XML con XAdES");
            return Errores.Firma.ErrorFirma;
        }
        finally
        {
            cert.Dispose();
        }
    }
}
