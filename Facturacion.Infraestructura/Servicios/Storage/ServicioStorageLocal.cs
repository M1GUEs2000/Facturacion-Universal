using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Servicios;
using Microsoft.Extensions.Options;

namespace Facturacion.Infraestructura.Servicios.Storage;

public class ServicioStorageLocal(IOptions<StorageLocalOpciones> opciones) : IServicioStorage
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
        catch (Exception)
        {
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
        catch (Exception)
        {
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
        catch (Exception)
        {
            return Task.FromResult<ErrorOr<bool>>(Errores.Storage.ErrorGuardar);
        }
    }

    private string BuildPath(string ruta) =>
        Path.GetFullPath(Path.Combine(_basePath, ruta));
}
