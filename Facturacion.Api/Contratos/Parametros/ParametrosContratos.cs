using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using FluentValidation;

namespace Facturacion.Api.Contratos.Parametros;

public record SecuencialSriRequest(
    string TipoComprobante,
    long Secuencial,
    string CodigoNumerico);

public record SecuencialSriResponse(
    Guid Id,
    string EmpresaRuc,
    string TipoComprobante,
    long Secuencial,
    string CodigoNumerico,
    DateTimeOffset FechaActualizacion)
{
    public static SecuencialSriResponse From(SecuencialSri p) =>
        new(p.Id, p.EmpresaRuc, p.TipoComprobante, p.Secuencial, p.CodigoNumerico, p.FechaActualizacion);
}

public record ParametrosFacturacionRequest(
    Ambiente Ambiente,
    string TipoEmision,
    bool AgenteRetencion,
    string? ContribuyenteRimpe,
    string Estab,
    string PuntoEmision,
    string? ContribuyenteEspecial,
    bool ObligadoContabilidad,
    string Moneda,
    string CodigoImpuesto,
    CodigoIva CodigoPorcentaje);

public record ParametrosFacturacionResponse(
    string EmpresaRuc,
    Ambiente Ambiente,
    string TipoEmision,
    bool AgenteRetencion,
    string? ContribuyenteRimpe,
    string Estab,
    string PuntoEmision,
    string? ContribuyenteEspecial,
    bool ObligadoContabilidad,
    string Moneda,
    string CodigoImpuesto,
    CodigoIva CodigoPorcentaje,
    DateTimeOffset FechaActualizacion)
{
    public static ParametrosFacturacionResponse From(ParametrosFacturacion p) =>
        new(
            p.EmpresaRuc, p.Ambiente, p.TipoEmision, p.AgenteRetencion,
            p.ContribuyenteRimpe, p.Estab, p.PuntoEmision,
            p.ContribuyenteEspecial, p.ObligadoContabilidad, p.Moneda,
            p.CodigoImpuesto, p.CodigoPorcentaje, p.FechaActualizacion);
}

public class SecuencialSriRequestValidator : AbstractValidator<SecuencialSriRequest>
{
    private static readonly string[] TiposPermitidos =
    [
        TipoDocumentoSri.Factura,
        TipoDocumentoSri.NotaCredito,
        TipoDocumentoSri.Retencion
    ];

    public SecuencialSriRequestValidator()
    {
        RuleFor(x => x.TipoComprobante).NotEmpty().Must(TiposPermitidos.Contains)
            .WithMessage("TipoComprobante debe ser 01, 04 o 07.");
        RuleFor(x => x.Secuencial).GreaterThan(0);
        RuleFor(x => x.CodigoNumerico).NotEmpty().Length(8).Matches(@"^\d+$");
    }
}

public class ParametrosFacturacionRequestValidator : AbstractValidator<ParametrosFacturacionRequest>
{
    private static readonly string[] RimpePermitidos =
    [
        "",
        "CONTRIBUYENTE REGIMEN RIMPE",
        "CONTRIBUYENTE NEGOCIO POPULAR - REGIMEN RIMPE"
    ];

    public ParametrosFacturacionRequestValidator()
    {
        RuleFor(x => x.Ambiente).IsInEnum();
        RuleFor(x => x.TipoEmision).NotEmpty().Equal("1");
        RuleFor(x => x.ContribuyenteRimpe ?? "").Must(RimpePermitidos.Contains)
            .WithMessage("ContribuyenteRimpe no es valido.");
        RuleFor(x => x.Estab).NotEmpty().Length(3).Matches(@"^\d+$");
        RuleFor(x => x.PuntoEmision).NotEmpty().Length(3).Matches(@"^\d+$");
        RuleFor(x => x.ContribuyenteEspecial)
            .Matches(@"^\d{3,13}$")
            .When(x => !string.IsNullOrWhiteSpace(x.ContribuyenteEspecial))
            .WithMessage("ContribuyenteEspecial debe tener entre 3 y 13 digitos.");
        RuleFor(x => x.Moneda).NotEmpty().Equal("USD");
        RuleFor(x => x.CodigoImpuesto).NotEmpty().Equal("2");
        RuleFor(x => x.CodigoPorcentaje).IsInEnum();
    }
}
