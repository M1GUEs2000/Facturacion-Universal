using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using FluentValidation;

namespace Facturacion.Api.Contratos.Facturas;

public record FormaPagoRequest(
    string Codigo,
    decimal Total,
    int? Plazo = null,
    string? UnidadTiempo = null);

public record InfoAdicionalRequest(string Nombre, string Valor);

public record DetalleFacturaRequest(
    int Orden,
    string CodigoPrincipal,
    string? CodigoAuxiliar,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Descuento,
    decimal PrecioTotalSinImpuesto,
    string? IceCodigo,
    decimal? IceTarifa,
    decimal? IceBase,
    decimal? IceValor,
    CodigoIva IvaCodigo,
    decimal IvaTarifa,
    decimal IvaBase,
    decimal IvaValor);

public record EmitirFacturaRequest(
    string EmpresaRuc,
    Ambiente Ambiente,
    string Estab,
    string PtoEmi,
    string? Secuencial,
    DateOnly FechaEmision,
    string TipoIdentificacionComprador,
    string IdentificacionComprador,
    string RazonSocialComprador,
    string? DireccionComprador,
    string? DirEstablecimiento,
    decimal TotalSinImpuestos,
    decimal TotalDescuento,
    decimal? BaseImponibleIce,
    decimal? ValorIce,
    decimal BaseImponibleIva,
    decimal ValorIva,
    decimal Propina,
    decimal ImporteTotal,
    string? GuiaRemision,
    List<FormaPagoRequest> FormasPago,
    List<InfoAdicionalRequest> InfoAdicional,
    List<DetalleFacturaRequest> Detalle);

public record FacturaResponse(
    Guid Id,
    string ClaveAcceso,
    string EstadoSri,
    string? NumeroAutorizacion,
    DateTimeOffset? FechaAutorizacion,
    bool HasPdf,
    bool HasXml)
{
    public static FacturaResponse From(Factura f) =>
        new(f.Id, f.ClaveAcceso, f.EstadoSri.ToString(), f.NumeroAutorizacion,
            f.FechaAutorizacion, f.PdfPath is not null, f.XmlAutorizadoPath is not null);
}

public class EmitirFacturaValidator : AbstractValidator<EmitirFacturaRequest>
{
    public EmitirFacturaValidator()
    {
        RuleFor(x => x.EmpresaRuc).NotEmpty();
        RuleFor(x => x.Estab).NotEmpty().Length(3).Matches(@"^\d+$");
        RuleFor(x => x.PtoEmi).NotEmpty().Length(3).Matches(@"^\d+$");
        RuleFor(x => x.Secuencial).Length(9).Matches(@"^\d+$").When(x => x.Secuencial is not null);
        RuleFor(x => x.TipoIdentificacionComprador)
            .NotEmpty()
            .Must(TipoIdentificacion.CompradorPermitidos.Contains)
            .WithMessage("TipoIdentificacionComprador no es un codigo SRI valido.");
        RuleFor(x => x.IdentificacionComprador).NotEmpty();
        RuleFor(x => x.RazonSocialComprador).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ImporteTotal).GreaterThan(0);
        RuleFor(x => x.FormasPago).NotEmpty();
        RuleFor(x => x.Detalle).NotEmpty();
        RuleForEach(x => x.Detalle).ChildRules(d =>
        {
            d.RuleFor(x => x.CodigoPrincipal).NotEmpty();
            d.RuleFor(x => x.Descripcion).NotEmpty();
            d.RuleFor(x => x.Cantidad).GreaterThan(0);
            d.RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0);
            d.RuleFor(x => x.IvaCodigo).IsInEnum();
        });
    }
}
