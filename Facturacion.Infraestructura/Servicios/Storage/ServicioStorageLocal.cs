using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Servicios;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Facturacion.Infraestructura.Servicios.Storage;

public class ServicioStorageLocal(IOptions<StorageLocalOpciones> opciones, ILogger<ServicioStorageLocal> logger) : IServicioStorage
{
    private readonly string _basePath = opciones.Value.BasePath;

    public async Task<ErrorOr<string>> GuardarAsync(byte[] contenido, string ruta, CancellationToken ct = default)
    {
        try
        {
            var fullPath = BuildPath(ruta);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await File.WriteAllBytesAsync(fullPath, contenido, ct);
            return ruta;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error guardando archivo en {Ruta}", ruta);
            return Errores.Storage.ErrorGuardar;
        }
    }

    public async Task<ErrorOr<byte[]>> ObtenerAsync(string ruta, CancellationToken ct = default)
    {
        try
        {
            var fullPath = BuildPath(ruta);
            if (!File.Exists(fullPath))
                return Errores.Storage.ArchivoNoEncontrado;

            return await File.ReadAllBytesAsync(fullPath, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo archivo de {Ruta}", ruta);
            return Errores.Storage.ArchivoNoEncontrado;
        }
    }

    public Task<ErrorOr<bool>> EliminarAsync(string ruta, CancellationToken ct = default)
    {
        try
        {
            var fullPath = BuildPath(ruta);
            if (!File.Exists(fullPath))
                return Task.FromResult<ErrorOr<bool>>(Errores.Storage.ArchivoNoEncontrado);

            File.Delete(fullPath);
            return Task.FromResult<ErrorOr<bool>>(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error eliminando archivo de {Ruta}", ruta);
            return Task.FromResult<ErrorOr<bool>>(Errores.Storage.ErrorGuardar);
        }
    }

    public Task<ErrorOr<string>> GenerarUrlFirmadaAsync(string ruta, int ttlSegundos = 300, CancellationToken ct = default) =>
        Task.FromResult<ErrorOr<string>>(Errores.Storage.UrlFirmadaNoSoportada);

    private string BuildPath(string ruta) =>
        Path.GetFullPath(Path.Combine(_basePath, ruta));
}
