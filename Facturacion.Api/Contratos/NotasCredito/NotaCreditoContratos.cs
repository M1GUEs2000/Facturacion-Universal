using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using FluentValidation;

namespace Facturacion.Api.Contratos.NotasCredito;

public record InfoAdicionalRequest(string Nombre, string Valor);

public record DetalleNotaCreditoRequest(
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

public record EmitirNotaCreditoRequest(
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
    string DocModificadoTipo,
    string DocModificadoNumero,
    DateOnly DocModificadoFecha,
    string DocModificadoClaveAcceso,
    string Motivo,
    decimal TotalSinImpuestos,
    decimal TotalDescuento,
    decimal? BaseImponibleIce,
    decimal? ValorIce,
    decimal BaseImponibleIva,
    decimal ValorIva,
    decimal ValorModificacion,
    List<InfoAdicionalRequest> InfoAdicional,
    List<DetalleNotaCreditoRequest> Detalle);

public record NotaCreditoResponse(
    Guid Id,
    string ClaveAcceso,
    string EstadoSri,
    string? NumeroAutorizacion,
    DateTimeOffset? FechaAutorizacion,
    bool HasPdf,
    bool HasXml)
{
    public static NotaCreditoResponse From(NotaCredito n) =>
        new(n.Id, n.ClaveAcceso, n.EstadoSri.ToString(), n.NumeroAutorizacion,
            n.FechaAutorizacion, n.PdfPath is not null, n.XmlAutorizadoPath is not null);
}

public class EmitirNotaCreditoValidator : AbstractValidator<EmitirNotaCreditoRequest>
{
    public EmitirNotaCreditoValidator()
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
        RuleFor(x => x.DocModificadoTipo).NotEmpty();
        RuleFor(x => x.DocModificadoNumero).NotEmpty();
        RuleFor(x => x.DocModificadoClaveAcceso).NotEmpty().Length(49);
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Detalle).NotEmpty();
        RuleForEach(x => x.Detalle).ChildRules(d =>
        {
            d.RuleFor(x => x.CodigoPrincipal).NotEmpty();
            d.RuleFor(x => x.Descripcion).NotEmpty();
            d.RuleFor(x => x.Cantidad).GreaterThan(0);
            d.RuleFor(x => x.IvaCodigo).IsInEnum();
        });
    }
}
