namespace Facturacion.Core.Enums;

public sealed record CodigoRetencionSri(
    string TipoImpuesto,
    string Codigo,
    string Descripcion,
    decimal Porcentaje);

public static class CatalogoRetencionesSri
{
    public const string TipoRenta = "1";
    public const string TipoIva = "2";

    public static readonly IReadOnlyList<CodigoRetencionSri> Renta =
    [
        new(TipoRenta, "303", "Honorarios profesionales", 10m),
        new(TipoRenta, "303A", "Servicios profesionales sociedades", 3m),
        new(TipoRenta, "304", "Servicios intelectuales no relacionados", 10m),
        new(TipoRenta, "304A", "Comisiones servicios intelectuales", 10m),
        new(TipoRenta, "304B", "Notarios y registradores", 10m),
        new(TipoRenta, "304C", "Deportistas, arbitros, entrenadores", 8m),
        new(TipoRenta, "304D", "Artistas", 8m),
        new(TipoRenta, "304E", "Servicios de docencia", 10m),
        new(TipoRenta, "307", "Servicios mano de obra", 2m),
        new(TipoRenta, "308", "Uso de imagen o renombre", 10m),
        new(TipoRenta, "309", "Publicidad y medios", 2.75m),
        new(TipoRenta, "310", "Transporte pasajeros o carga", 1m),
        new(TipoRenta, "311", "Liquidacion de compra", 2m),
        new(TipoRenta, "312", "Transferencia bienes muebles", 1.75m),
        new(TipoRenta, "312A", "Compras productor", 1m),
        new(TipoRenta, "312C", "Compras comercializador", 1.75m),
        new(TipoRenta, "314A", "Regalias franquicias PN", 10m),
        new(TipoRenta, "314B", "Derechos autor PN", 10m),
        new(TipoRenta, "314C", "Regalias franquicias sociedades", 10m),
        new(TipoRenta, "314D", "Derechos autor sociedades", 10m),
        new(TipoRenta, "319", "Arrendamiento mercantil", 2m),
        new(TipoRenta, "320", "Arrendamiento bienes inmuebles", 10m),
        new(TipoRenta, "322", "Seguros y reaseguros", 1m),
        new(TipoRenta, "323", "Rendimientos financieros", 2m),
        new(TipoRenta, "323E", "Depositos plazo fijo gravados", 2m),
        new(TipoRenta, "323E2", "Depositos plazo fijo exentos", 0m),
        new(TipoRenta, "325", "Anticipo dividendos", 25m),
        new(TipoRenta, "325A", "Prestamos accionistas", 25m),
        new(TipoRenta, "332", "No sujeto a retencion", 0m),
        new(TipoRenta, "343", "Otras retenciones 1%", 1m),
        new(TipoRenta, "343A", "Energia electrica", 1m),
        new(TipoRenta, "343B", "Construccion", 1.75m),
        new(TipoRenta, "343C", "Botellas PET", 2m),
        new(TipoRenta, "3440", "Otras retenciones 2.75%", 2.75m),
        new(TipoRenta, "3480", "Pronosticos deportivos", 15m),
        new(TipoRenta, "3482", "Comisiones sociedades", 3m)
    ];

    public static readonly IReadOnlyList<CodigoRetencionSri> Iva =
    [
        new(TipoIva, "9", "Retencion IVA 10%", 10m),
        new(TipoIva, "10", "Retencion IVA 20%", 20m),
        new(TipoIva, "1", "Retencion IVA 30%", 30m),
        new(TipoIva, "11", "Retencion IVA 50%", 50m),
        new(TipoIva, "2", "Retencion IVA 70%", 70m),
        new(TipoIva, "3", "Retencion IVA 100%", 100m),
        new(TipoIva, "7", "Retencion IVA 0%", 0m),
        new(TipoIva, "8", "No procede retencion", 0m)
    ];

    public static readonly IReadOnlyList<CodigoRetencionSri> Todos = [.. Renta, .. Iva];

    public static bool EsTipoImpuestoValido(string codigoImpuesto) =>
        codigoImpuesto is TipoRenta or TipoIva;

    public static bool EsCodigoRetencionValido(string codigoImpuesto, string codigoRetencion) =>
        Todos.Any(c =>
            c.TipoImpuesto == codigoImpuesto
            && string.Equals(c.Codigo, codigoRetencion, StringComparison.OrdinalIgnoreCase));
}
