using Facturacion.Core.Enums;

namespace Facturacion.Core.Entidades;

public class FacturaDetalle
{
    protected FacturaDetalle() { }

    public Guid Id { get; private set; }
    public Guid FacturaId { get; private set; }
    public int Orden { get; private set; }
    public string CodigoPrincipal { get; private set; } = null!;
    public string? CodigoAuxiliar { get; private set; }
    public string Descripcion { get; private set; } = null!;
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Descuento { get; private set; }
    public decimal PrecioTotalSinImpuesto { get; private set; }
    public string? IceCodigo { get; private set; }
    public decimal? IceTarifa { get; private set; }
    public decimal? IceBase { get; private set; }
    public decimal? IceValor { get; private set; }
    public CodigoIva IvaCodigo { get; private set; }
    public decimal IvaTarifa { get; private set; }
    public decimal IvaBase { get; private set; }
    public decimal IvaValor { get; private set; }

    public static FacturaDetalle Crear(
        Guid facturaId,
        int orden,
        string codigoPrincipal,
        string? codigoAuxiliar,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal descuento,
        decimal precioTotalSinImpuesto,
        string? iceCodigo,
        decimal? iceTarifa,
        decimal? iceBase,
        decimal? iceValor,
        CodigoIva ivaCodigo,
        decimal ivaTarifa,
        decimal ivaBase,
        decimal ivaValor)
    {
        return new FacturaDetalle
        {
            Id = Guid.NewGuid(),
            FacturaId = facturaId,
            Orden = orden,
            CodigoPrincipal = codigoPrincipal,
            CodigoAuxiliar = codigoAuxiliar,
            Descripcion = descripcion,
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            Descuento = descuento,
            PrecioTotalSinImpuesto = precioTotalSinImpuesto,
            IceCodigo = iceCodigo,
            IceTarifa = iceTarifa,
            IceBase = iceBase,
            IceValor = iceValor,
            IvaCodigo = ivaCodigo,
            IvaTarifa = ivaTarifa,
            IvaBase = ivaBase,
            IvaValor = ivaValor
        };
    }
}
