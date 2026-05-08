namespace Facturacion.Core.Interfaces;

public interface IDocumentoEmitible
{
    void RegistrarXmlFirmado(string path);
    void RegistrarEnvioSri();
    void RegistrarNoAutorizacion(string? sriRespuesta = null);
    void RegistrarAutorizacion(string numeroAutorizacion, DateTimeOffset fechaAutorizacion,
        string xmlAutorizadoPath, string pdfPath, string? sriRespuesta = null);
}
