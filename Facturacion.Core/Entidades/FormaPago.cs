namespace Facturacion.Core.Entidades;

public record FormaPago(
    string Codigo,
    decimal Total,
    int? Plazo = null,
    string? UnidadTiempo = null);
