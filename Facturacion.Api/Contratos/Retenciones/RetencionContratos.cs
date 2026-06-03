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
    string? Secuencial,
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
    bool HasPdf,
    bool HasXml)
{
    public static RetencionResponse From(Retencion r) =>
        new(r.Id, r.ClaveAcceso, r.EstadoSri.ToString(), r.NumeroAutorizacion,
            r.FechaAutorizacion, r.PdfPath is not null, r.XmlAutorizadoPath is not null);
}

public class EmitirRetencionValidator : AbstractValidator<EmitirRetencionRequest>
{
    public EmitirRetencionValidator()
    {
        RuleFor(x => x.EmpresaRuc).NotEmpty();
        RuleFor(x => x.Estab).NotEmpty().Length(3).Matches(@"^\d+$");
        RuleFor(x => x.PtoEmi).NotEmpty().Length(3).Matches(@"^\d+$");
        RuleFor(x => x.Secuencial).Length(9).Matches(@"^\d+$").When(x => x.Secuencial is not null);
        RuleFor(x => x.TipoIdentificacionSujeto)
            .NotEmpty()
            .Must(TipoIdentificacion.SujetoRetenidoPermitidos.Contains)
            .WithMessage("TipoIdentificacionSujeto no es un codigo SRI valido para retenciones.");
        RuleFor(x => x.IdentificacionSujeto).NotEmpty();
        RuleFor(x => x.RazonSocialSujeto).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PeriodoFiscal).NotEmpty().Matches(@"^\d{2}/\d{4}$")
            .WithMessage("PeriodoFiscal debe tener formato MM/YYYY.");
        RuleFor(x => x.TotalRetenido).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Detalle).NotEmpty();
        RuleForEach(x => x.Detalle).ChildRules(d =>
        {
            d.RuleFor(x => x.CodigoImpuesto)
                .NotEmpty()
                .Must(CatalogoRetencionesSri.EsTipoImpuestoValido)
                .WithMessage("CodigoImpuesto debe ser 1 (RENTA) o 2 (IVA).");
            d.RuleFor(x => x.CodigoRetencion)
                .NotEmpty()
                .Must((detalle, codigoRetencion) =>
                    CatalogoRetencionesSri.EsCodigoRetencionValido(detalle.CodigoImpuesto, codigoRetencion))
                .WithMessage("CodigoRetencion no corresponde al catalogo SRI del impuesto seleccionado.");
            d.RuleFor(x => x.BaseImponible).GreaterThan(0);
            d.RuleFor(x => x.PorcentajeRetener).GreaterThanOrEqualTo(0);
            d.RuleFor(x => x.ValorRetenido).GreaterThanOrEqualTo(0);
            d.RuleFor(x => x.NumDocSustento).NotEmpty();
        });
    }
}
