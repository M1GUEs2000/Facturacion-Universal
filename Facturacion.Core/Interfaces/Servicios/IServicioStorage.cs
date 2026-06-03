using ErrorOr;

namespace Facturacion.Core.Interfaces.Servicios;

public interface IServicioStorage
{
    Task<ErrorOr<string>> GuardarAsync(byte[] contenido, string ruta, CancellationToken ct = default);
    Task<ErrorOr<byte[]>> ObtenerAsync(string ruta, CancellationToken ct = default);
    Task<ErrorOr<bool>> EliminarAsync(string ruta, CancellationToken ct = default);
    Task<ErrorOr<string>> GenerarUrlFirmadaAsync(string ruta, int ttlSegundos = 300, CancellationToken ct = default);
}
