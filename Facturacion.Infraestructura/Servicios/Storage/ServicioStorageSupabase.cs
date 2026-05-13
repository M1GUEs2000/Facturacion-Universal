using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Servicios;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Facturacion.Infraestructura.Servicios.Storage;

public class ServicioStorageSupabase(
    IHttpClientFactory httpFactory,
    SupabaseStorageOpciones opciones,
    ILogger<ServicioStorageSupabase> logger) : IServicioStorage, IServicioStorageFirmaYLogo
{
    public async Task<ErrorOr<string>> GuardarAsync(byte[] contenido, string ruta, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var url = $"{opciones.Url}/storage/v1/object/{opciones.Bucket}/{ruta}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-upsert", "true");
            request.Content = new ByteArrayContent(contenido);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(ruta));

            var response = await client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogError("Supabase Storage error {Status} guardando {Ruta}: {Body}", response.StatusCode, ruta, body);
                return Errores.Storage.ErrorGuardar;
            }

            return ruta;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error guardando {Ruta} en Supabase Storage", ruta);
            return Errores.Storage.ErrorGuardar;
        }
    }

    public async Task<ErrorOr<byte[]>> ObtenerAsync(string ruta, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var url = $"{opciones.Url}/storage/v1/object/{opciones.Bucket}/{ruta}";

            var response = await client.GetAsync(url, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Errores.Storage.ArchivoNoEncontrado;

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Supabase Storage error {Status} obteniendo {Ruta}", response.StatusCode, ruta);
                return Errores.Storage.ArchivoNoEncontrado;
            }

            return await response.Content.ReadAsByteArrayAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo {Ruta} de Supabase Storage", ruta);
            return Errores.Storage.ArchivoNoEncontrado;
        }
    }

    public async Task<ErrorOr<bool>> EliminarAsync(string ruta, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var url = $"{opciones.Url}/storage/v1/object/{opciones.Bucket}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Content = new StringContent(
                $"{{\"prefixes\":[\"{ruta}\"]}}",
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Supabase Storage error {Status} eliminando {Ruta}", response.StatusCode, ruta);
                return Errores.Storage.ArchivoNoEncontrado;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error eliminando {Ruta} de Supabase Storage", ruta);
            return Errores.Storage.ArchivoNoEncontrado;
        }
    }

    private HttpClient CreateClient()
    {
        var client = httpFactory.CreateClient("supabase-storage");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", opciones.ServiceRoleKey);
        return client;
    }

    private static string GetContentType(string ruta) =>
        Path.GetExtension(ruta).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".xml" => "application/xml",
            ".p12" => "application/x-pkcs12",
            _ => "application/octet-stream"
        };
}
