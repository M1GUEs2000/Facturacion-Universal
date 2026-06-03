using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces;

namespace Facturacion.Core.Entidades;

public abstract class DocumentoElectronico : IDocumentoEmitible
{
    protected DocumentoElectronico() { }

    public Guid Id { get; protected set; }
    public string EmpresaRuc { get; protected set; } = null!;
    public string? IpAddress { get; protected set; }
    public Ambiente Ambiente { get; protected set; }
    public string Estab { get; protected set; } = null!;
    public string PtoEmi { get; protected set; } = null!;
    public string Secuencial { get; protected set; } = null!;
    public string ClaveAcceso { get; protected set; } = null!;
    public DateOnly FechaEmision { get; protected set; }
    public List<InfoAdicional> InfoAdicional { get; protected set; } = [];
    public EstadoSri EstadoSri { get; protected set; }
    public EstadoCorreo EstadoCorreo { get; protected set; }
    public string? NumeroAutorizacion { get; protected set; }
    public DateTimeOffset? FechaAutorizacion { get; protected set; }
    public string? SriRespuesta { get; protected set; }
    public string? XmlFirmadoPath { get; protected set; }
    public string? XmlAutorizadoPath { get; protected set; }
    public string? PdfPath { get; protected set; }
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }

    public void RegistrarXmlFirmado(string path)
    {
        XmlFirmadoPath = path;
        EstadoSri = EstadoSri.Enviado;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RegistrarEnvioSri()
    {
        EstadoSri = EstadoSri.PendienteAutorizacion;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RegistrarNumeroAutorizacion(string numeroAutorizacion, DateTimeOffset fechaAutorizacion, string? sriRespuesta = null)
    {
        NumeroAutorizacion = numeroAutorizacion;
        FechaAutorizacion = fechaAutorizacion;
        SriRespuesta = sriRespuesta;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RegistrarAutorizacionSri(
        string numeroAutorizacion,
        DateTimeOffset fechaAutorizacion,
        string xmlAutorizadoPath,
        string? sriRespuesta = null)
    {
        NumeroAutorizacion = numeroAutorizacion;
        FechaAutorizacion = fechaAutorizacion;
        XmlAutorizadoPath = xmlAutorizadoPath;
        XmlFirmadoPath = null;
        SriRespuesta = sriRespuesta;
        EstadoSri = EstadoSri.AutorizadoPendienteArchivos;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RegistrarPdf(string pdfPath)
    {
        PdfPath = pdfPath;
        EstadoSri = EstadoSri.Autorizado;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RegistrarNoAutorizacion(string? sriRespuesta = null)
    {
        SriRespuesta = sriRespuesta;
        EstadoSri = EstadoSri.NoAutorizado;
        XmlFirmadoPath = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Anular()
    {
        EstadoSri = EstadoSri.Anulado;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarcarCorreoEnviado()
    {
        EstadoCorreo = EstadoCorreo.Enviado;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
