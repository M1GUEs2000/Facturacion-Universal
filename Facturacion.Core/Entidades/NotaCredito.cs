namespace Facturacion.Core.Entidades;

public class NotaCredito : DocumentoElectronico
{
    protected NotaCredito() { }

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
    public IReadOnlyList<NotaCreditoDetalle> Detalle { get; private set; } = [];

    public static NotaCredito Crear(
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
        string? ipAddress = null,
        Guid? id = null)
    {
        return new NotaCredito
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
            EstadoSri = Enums.EstadoSri.Pendiente,
            EstadoCorreo = Enums.EstadoCorreo.Pendiente,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
