namespace Facturacion.Core.Entidades;

public class RetencionDetalle
{
    protected RetencionDetalle() { }

    public Guid Id { get; private set; }
    public Guid RetencionId { get; private set; }
    public int Orden { get; private set; }
    public string CodigoImpuesto { get; private set; } = null!;
    public string CodigoRetencion { get; private set; } = null!;
    public decimal BaseImponible { get; private set; }
    public decimal PorcentajeRetener { get; private set; }
    public decimal ValorRetenido { get; private set; }
    public string CodDocSustento { get; private set; } = null!;
    public string NumDocSustento { get; private set; } = null!;
    public DateOnly FechaEmisionDocSustento { get; private set; }

    public static RetencionDetalle Crear(
        Guid retencionId,
        int orden,
        string codigoImpuesto,
        string codigoRetencion,
        decimal baseImponible,
        decimal porcentajeRetener,
        decimal valorRetenido,
        string codDocSustento,
        string numDocSustento,
        DateOnly fechaEmisionDocSustento)
    {
        return new RetencionDetalle
        {
            Id = Guid.NewGuid(),
            RetencionId = retencionId,
            Orden = orden,
            CodigoImpuesto = codigoImpuesto,
            CodigoRetencion = codigoRetencion,
            BaseImponible = baseImponible,
            PorcentajeRetener = porcentajeRetener,
            ValorRetenido = valorRetenido,
            CodDocSustento = codDocSustento,
            NumDocSustento = numDocSustento,
            FechaEmisionDocSustento = fechaEmisionDocSustento
        };
    }
}
