using Facturacion.Core.Enums;

namespace Facturacion.Core.Interfaces;

public interface IDocumentoEmitible
{
    string EmpresaRuc { get; }
    EstadoSri EstadoSri { get; }
    string? XmlFirmadoPath { get; }
    string? XmlAutorizadoPath { get; }
    string? PdfPath { get; }
    string? NumeroAutorizacion { get; }

    void RegistrarXmlFirmado(string path);
    void RegistrarEnvioSri();
    void RegistrarNoAutorizacion(string? sriRespuesta = null);
    // Persiste el número de autorización SRI inmediatamente; EstadoSri permanece PendienteAutorizacion
    // hasta que el XML autorizado se guarde exitosamente en storage (RegistrarAutorizacionSri).
    void RegistrarNumeroAutorizacion(string numeroAutorizacion, DateTimeOffset fechaAutorizacion, string? sriRespuesta = null);
    void RegistrarAutorizacionSri(string numeroAutorizacion, DateTimeOffset fechaAutorizacion,
        string xmlAutorizadoPath, string? sriRespuesta = null);
    void RegistrarPdf(string pdfPath);
}
