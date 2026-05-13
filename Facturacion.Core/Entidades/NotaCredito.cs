using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces;

namespace Facturacion.Core.Entidades;

public class NotaCredito : IDocumentoEmitible
{
    protected NotaCredito() { }

    public Guid Id { get; private set; }
    public string EmpresaRuc { get; private set; } = null!;
    public string? IpAddress { get; private set; }
    public Ambiente Ambiente { get; private set; }
    public string Estab { get; private set; } = null!;
    public string PtoEmi { get; private set; } = null!;
    public string Secuencial { get; private set; } = null!;
    public string ClaveAcceso { get; private set; } = null!;
    public DateOnly FechaEmision { get; private set; }
    public string TipoIdentificacionComprador { get; private set; } = null!;
    public string IdentificacionComprador { get; private set; } = null!;
    public string RazonSocialComprador { get; private set; } = null!;
    public string? DireccionComprador { get; private set; }
    public string? DirEstablecimiento { get; private set; }
    public string DocModificadoTipo { get; private set; } = null!;
    public string DocModificadoNumero { get; private set; } = null!;
    public DateOnly DocModificadoFecha { get; private set; }
    public string DocModificadoClaveAcceso { get; private set; } = null!;
    public string Motivo { get; private set; } = null!;
    public decimal TotalSinImpuestos { get; private set; }
    public decimal TotalDescuento { get; private set; }
    public decimal? BaseImponibleIce { get; private set; }
    public decimal? ValorIce { get; private set; }
    public decimal BaseImponibleIva { get; private set; }
    public decimal ValorIva { get; private set; }
    public decimal ValorModificacion { get; private set; }
    public List<InfoAdicional> InfoAdicional { get; private set; } = [];
    public EstadoSri EstadoSri { get; private set; }
    public EstadoCorreo EstadoCorreo { get; private set; }
    public string? NumeroAutorizacion { get; private set; }
    public DateTimeOffset? FechaAutorizacion { get; private set; }
    public string? SriRespuesta { get; private set; }
    public string? XmlFirmadoPath { get; private set; }
    public string? XmlAutorizadoPath { get; private set; }
    public string? PdfPath { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<NotaCreditoDetalle> Detalle { get; private set; } = [];

    public static NotaCredito Crear(
        string empresaRuc,
        Ambiente ambiente,
        string estab,
        string ptoEmi,
        string secuencial,
        string claveAcceso,
        DateOnly fechaEmision,
        string tipoIdentificacionComprador,
        string identificacionComprador,
        string razonSocialComprador,
        string? direccionComprador,
        string? dirEstablecimiento,
        string docModificadoTipo,
        string docModificadoNumero,
        DateOnly docModificadoFecha,
        string docModificadoClaveAcceso,
        string motivo,
        decimal totalSinImpuestos,
        decimal totalDescuento,
        decimal? baseImponibleIce,
        decimal? valorIce,
        decimal baseImponibleIva,
        decimal valorIva,
        decimal valorModificacion,
        List<InfoAdicional> infoAdicional,
        List<NotaCreditoDetalle> detalle,
        string? ipAddress = null)
    {
        return new NotaCredito
        {
            Id = Guid.NewGuid(),
            EmpresaRuc = empresaRuc,
            IpAddress = ipAddress,
            Ambiente = ambiente,
            Estab = estab,
            PtoEmi = ptoEmi,
            Secuencial = secuencial,
            ClaveAcceso = claveAcceso,
            FechaEmision = fechaEmision,
            TipoIdentificacionComprador = tipoIdentificacionComprador,
            IdentificacionComprador = identificacionComprador,
            RazonSocialComprador = razonSocialComprador,
            DireccionComprador = direccionComprador,
            DirEstablecimiento = dirEstablecimiento,
            DocModificadoTipo = docModificadoTipo,
            DocModificadoNumero = docModificadoNumero,
            DocModificadoFecha = docModificadoFecha,
            DocModificadoClaveAcceso = docModificadoClaveAcceso,
            Motivo = motivo,
            TotalSinImpuestos = totalSinImpuestos,
            TotalDescuento = totalDescuento,
            BaseImponibleIce = baseImponibleIce,
            ValorIce = valorIce,
            BaseImponibleIva = baseImponibleIva,
            ValorIva = valorIva,
            ValorModificacion = valorModificacion,
            InfoAdicional = infoAdicional,
            Detalle = detalle,
            EstadoSri = EstadoSri.Pendiente,
            EstadoCorreo = EstadoCorreo.Pendiente,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

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
