using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using FluentValidation;

namespace Facturacion.Api.Contratos.Retenciones;

public record InfoAdicionalRequest(string Nombre, string Valor);

public record DetalleRetencionRequest(
    int Orden,
    string CodigoImpuesto,
    string CodigoRetencion,
    decimal BaseImponible,
    decimal PorcentajeRetener,
    decimal ValorRetenido,
    string CodDocSustento,
    string NumDocSustento,
    DateOnly FechaEmisionDocSustento);

public record EmitirRetencionRequest(
    string EmpresaRuc,
    Ambiente Ambiente,
    string Estab,
    string PtoEmi,
    string Secuencial,
    DateOnly FechaEmision,
    string TipoIdentificacionSujeto,
    string IdentificacionSujeto,
    string RazonSocialSujeto,
    string? DireccionSujeto,
    string PeriodoFiscal,
    decimal TotalBaseImponible,
    decimal TotalRetencionRenta,
    decimal TotalRetencionIva,
    decimal TotalRetenido,
    List<InfoAdicionalRequest> InfoAdicional,
    List<DetalleRetencionRequest> Detalle);

public record RetencionResponse(
    Guid Id,
    string ClaveAcceso,
    string EstadoSri,
    string? NumeroAutorizacion,
    DateTimeOffset? FechaAutorizacion,
    string? XmlAutorizadoPath,
    string? PdfPath)
{
    public static RetencionResponse From(Retencion r) =>
        new(r.Id, r.ClaveAcceso, r.EstadoSri.ToString(), r.NumeroAutorizacion,
            r.FechaAutorizacion, r.XmlAutorizadoPath, r.PdfPath);
}

public class EmitirRetencionValidator : AbstractValidator<EmitirRetencionRequest>
{
    public EmitirRetencionValidator()
    {
        RuleFor(x => x.EmpresaRuc).NotEmpty();
        RuleFor(x => x.Estab).NotEmpty().Length(3).Matches(@"^\d+$");
        RuleFor(x => x.PtoEmi).NotEmpty().Length(3).Matches(@"^\d+$");
        RuleFor(x => x.Secuencial).NotEmpty().Length(9).Matches(@"^\d+$");
        RuleFor(x => x.TipoIdentificacionSujeto).NotEmpty();
        RuleFor(x => x.IdentificacionSujeto).NotEmpty();
        RuleFor(x => x.RazonSocialSujeto).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PeriodoFiscal).NotEmpty().Matches(@"^\d{2}/\d{4}$")
            .WithMessage("PeriodoFiscal debe tener formato MM/YYYY.");
        RuleFor(x => x.TotalRetenido).GreaterThan(0);
        RuleFor(x => x.Detalle).NotEmpty();
        RuleForEach(x => x.Detalle).ChildRules(d =>
        {
            d.RuleFor(x => x.CodigoImpuesto).NotEmpty();
            d.RuleFor(x => x.CodigoRetencion).NotEmpty();
            d.RuleFor(x => x.BaseImponible).GreaterThan(0);
            d.RuleFor(x => x.NumDocSustento).NotEmpty();
        });
    }
}
