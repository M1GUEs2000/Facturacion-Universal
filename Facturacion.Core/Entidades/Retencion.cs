namespace Facturacion.Core.Entidades;

public class Retencion : DocumentoElectronico
{
    protected Retencion() { }

    public string TipoIdentificacionSujeto { get; private set; } = null!;
    public string IdentificacionSujeto { get; private set; } = null!;
    public string RazonSocialSujeto { get; private set; } = null!;
    public string? DireccionSujeto { get; private set; }
    public string PeriodoFiscal { get; private set; } = null!;
    public decimal TotalBaseImponible { get; private set; }
    public decimal TotalRetencionRenta { get; private set; }
    public decimal TotalRetencionIva { get; private set; }
    public decimal TotalRetenido { get; private set; }
    public List<RetencionDetalle> Detalle { get; private set; } = [];

    public static Retencion Crear(
        string empresaRuc,
        Enums.Ambiente ambiente,
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
        string? ipAddress = null,
        Guid? id = null)
    {
        return new Retencion
        {
            Id = id ?? Guid.NewGuid(),
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
            EstadoSri = Enums.EstadoSri.Pendiente,
            EstadoCorreo = Enums.EstadoCorreo.Pendiente,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
