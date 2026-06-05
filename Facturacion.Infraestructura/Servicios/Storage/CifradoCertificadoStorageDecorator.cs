using ErrorOr;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Infraestructura.Seguridad;

namespace Facturacion.Infraestructura.Servicios.Storage;

/// <summary>
/// Cifra el P12 con AES-256-GCM antes de guardarlo y lo descifra al obtenerlo.
/// Solo aplica a rutas que terminan en ".p12"; el resto se delega sin modificar.
/// </summary>
public class CifradoCertificadoStorageDecorator(IServicioStorageFirmaYLogo inner) : IServicioStorageFirmaYLogo
{
    public async Task<ErrorOr<string>> GuardarAsync(byte[] contenido, string ruta, CancellationToken ct = default)
    {
        var payload = EsCertificado(ruta) ? CertPasswordEncryption.EncryptBytes(contenido) : contenido;
        return await inner.GuardarAsync(payload, ruta, ct);
    }

    public async Task<ErrorOr<byte[]>> ObtenerAsync(string ruta, CancellationToken ct = default)
    {
        var result = await inner.ObtenerAsync(ruta, ct);
        if (result.IsError || !EsCertificado(ruta)) return result;
        return CertPasswordEncryption.DecryptBytes(result.Value);
    }

    public Task<ErrorOr<bool>> EliminarAsync(string ruta, CancellationToken ct = default) =>
        inner.EliminarAsync(ruta, ct);

    public Task<ErrorOr<string>> GenerarUrlFirmadaAsync(string ruta, int ttlSegundos = 300, CancellationToken ct = default) =>
        inner.GenerarUrlFirmadaAsync(ruta, ttlSegundos, ct);

    private static bool EsCertificado(string ruta) =>
        ruta.EndsWith(".p12", StringComparison.OrdinalIgnoreCase);
}
