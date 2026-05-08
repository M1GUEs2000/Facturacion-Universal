using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces;

namespace Facturacion.Core.Entidades;

public class Retencion : IDocumentoEmitible
{
    protected Retencion() { }

    public Guid Id { get; private set; }
    public string EmpresaRuc { get; private set; } = null!;
    public string? IpAddress { get; private set; }
    public Ambiente Ambiente { get; private set; }
    public string Estab { get; private set; } = null!;
    public string PtoEmi { get; private set; } = null!;
    public string Secuencial { get; private set; } = null!;
    public string ClaveAcceso { get; private set; } = null!;
    public DateOnly FechaEmision { get; private set; }
    public string TipoIdentificacionSujeto { get; private set; } = null!;
    public string IdentificacionSujeto { get; private set; } = null!;
    public string RazonSocialSujeto { get; private set; } = null!;
    public string? DireccionSujeto { get; private set; }
    public string PeriodoFiscal { get; private set; } = null!;
    public decimal TotalBaseImponible { get; private set; }
    public decimal TotalRetencionRenta { get; private set; }
    public decimal TotalRetencionIva { get; private set; }
    public decimal TotalRetenido { get; private set; }
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
    public List<RetencionDetalle> Detalle { get; private set; } = [];

    public static Retencion Crear(
        string empresaRuc,
        Ambiente ambiente,
        string estab,
        string ptoEmi,
        string secuencial,
        string claveAcceso,
        DateOnly fechaEmision,
        string tipoIdentificacionSujeto,
        string identificacionSujeto,
        string razonSocialSujeto,
        string? direccionSujeto,
        string periodoFiscal,
        decimal totalBaseImponible,
        decimal totalRetencionRenta,
        decimal totalRetencionIva,
        decimal totalRetenido,
        List<InfoAdicional> infoAdicional,
        List<RetencionDetalle> detalle,
        string? ipAddress = null)
    {
        return new Retencion
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
            TipoIdentificacionSujeto = tipoIdentificacionSujeto,
            IdentificacionSujeto = identificacionSujeto,
            RazonSocialSujeto = razonSocialSujeto,
            DireccionSujeto = direccionSujeto,
            PeriodoFiscal = periodoFiscal,
            TotalBaseImponible = totalBaseImponible,
            TotalRetencionRenta = totalRetencionRenta,
            TotalRetencionIva = totalRetencionIva,
            TotalRetenido = totalRetenido,
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

    public void RegistrarAutorizacion(
        string numeroAutorizacion,
        DateTimeOffset fechaAutorizacion,
        string xmlAutorizadoPath,
        string pdfPath,
        string? sriRespuesta = null)
    {
        NumeroAutorizacion = numeroAutorizacion;
        FechaAutorizacion = fechaAutorizacion;
        XmlAutorizadoPath = xmlAutorizadoPath;
        PdfPath = pdfPath;
        SriRespuesta = sriRespuesta;
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
