namespace Facturacion.Core.Entidades;

public class Factura : DocumentoElectronico
{
    protected Factura() { }

    public string TipoIdentificacionComprador { get; private set; } = null!;
    public string IdentificacionComprador { get; private set; } = null!;
    public string RazonSocialComprador { get; private set; } = null!;
    public string? DireccionComprador { get; private set; }
    public string? DirEstablecimiento { get; private set; }
    public decimal TotalSinImpuestos { get; private set; }
    public decimal TotalDescuento { get; private set; }
    public decimal? BaseImponibleIce { get; private set; }
    public decimal? ValorIce { get; private set; }
    public decimal BaseImponibleIva { get; private set; }
    public decimal ValorIva { get; private set; }
    public decimal Propina { get; private set; }
    public decimal ImporteTotal { get; private set; }
    public string? GuiaRemision { get; private set; }
    public List<FormaPago> FormasPago { get; private set; } = [];
    public IReadOnlyList<FacturaDetalle> Detalle { get; private set; } = [];

    public static Factura Crear(
        string empresaRuc,
        Enums.Ambiente ambiente,
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
        decimal totalSinImpuestos,
        decimal totalDescuento,
        decimal? baseImponibleIce,
        decimal? valorIce,
        decimal baseImponibleIva,
        decimal valorIva,
        decimal propina,
        decimal importeTotal,
        string? guiaRemision,
        List<FormaPago> formasPago,
        List<InfoAdicional> infoAdicional,
        List<FacturaDetalle> detalle,
        string? ipAddress = null,
        Guid? id = null)
    {
        return new Factura
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
            TipoIdentificacionComprador = tipoIdentificacionComprador,
            IdentificacionComprador = identificacionComprador,
            RazonSocialComprador = razonSocialComprador,
            DireccionComprador = direccionComprador,
            DirEstablecimiento = dirEstablecimiento,
            TotalSinImpuestos = totalSinImpuestos,
            TotalDescuento = totalDescuento,
            BaseImponibleIce = baseImponibleIce,
            ValorIce = valorIce,
            BaseImponibleIva = baseImponibleIva,
            ValorIva = valorIva,
            Propina = propina,
            ImporteTotal = importeTotal,
            GuiaRemision = guiaRemision,
            FormasPago = formasPago,
            InfoAdicional = infoAdicional,
            Detalle = detalle,
            EstadoSri = Enums.EstadoSri.Pendiente,
            EstadoCorreo = Enums.EstadoCorreo.Pendiente,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
