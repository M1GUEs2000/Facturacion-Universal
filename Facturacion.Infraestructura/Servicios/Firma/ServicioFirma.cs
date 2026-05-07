using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Servicios;
using FirmaXadesNet;
using FirmaXadesNet.Crypto;
using FirmaXadesNet.Signature.Parameters;

namespace Facturacion.Infraestructura.Servicios.Firma;

public class ServicioFirma : IServicioFirma
{
    public Task<ErrorOr<string>> FirmarXmlAsync(
        string xml, byte[] certificadoP12, string password, CancellationToken ct = default)
        => Task.Run(() => Firmar(xml, certificadoP12, password), ct);

    private static ErrorOr<string> Firmar(string xml, byte[] certificadoP12, string password)
    {
        X509Certificate2 cert;
        try
        {
            cert = new X509Certificate2(
                certificadoP12,
                password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (CryptographicException)
        {
            return Errores.Firma.CertificadoInvalido;
        }

        try
        {
            var service = new XadesService();
            var parametros = new SignatureParameters
            {
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                InputMimeType = "text/xml",
                SignatureMethod = SignatureMethod.RSAwithSHA1,
                DigestMethod = DigestMethod.SHA1
            };

            using parametros.Signer = new Signer(cert);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var docFirmado = service.Sign(ms, parametros);
            return docFirmado.OuterXml;
        }
        catch (Exception)
        {
            return Errores.Firma.ErrorFirma;
        }
        finally
        {
            cert.Dispose();
        }
    }
}
