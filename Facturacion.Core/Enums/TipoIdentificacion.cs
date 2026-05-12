namespace Facturacion.Core.Enums;

// Codigos SRI para tipo de identificacion del comprador / sujeto retenido.
public static class TipoIdentificacion
{
    public const string Ruc = "04";
    public const string Cedula = "05";
    public const string Pasaporte = "06";
    public const string ConsumidorFinal = "07";
    public const string IdentificacionExterior = "08";
    public const string PlacaVehiculo = "09";

    public static readonly IReadOnlySet<string> CompradorPermitidos = new HashSet<string>
    {
        Ruc,
        Cedula,
        Pasaporte,
        ConsumidorFinal,
        IdentificacionExterior,
        PlacaVehiculo
    };

    public static readonly IReadOnlySet<string> SujetoRetenidoPermitidos = new HashSet<string>
    {
        Ruc,
        Cedula,
        Pasaporte
    };
}
