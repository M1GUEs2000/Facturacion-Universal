using Facturacion.Core.Entidades;
using FluentValidation;

namespace Facturacion.Api.Contratos.Empresas;

public record RegistrarEmpresaRequest(
    string Ruc,
    string Nombre,
    string DirMatriz,
    bool ObligadoContabilidad,
    string CertificadoP12Base64,
    string CertPassword,
    string? NombreComercial = null);

public record ActualizarCertificadoRequest(
    string CertificadoP12Base64,
    string CertPassword);

public record EmpresaResponse(
    string Ruc,
    string Nombre,
    string DirMatriz,
    string? NombreComercial,
    bool ObligadoContabilidad,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static EmpresaResponse From(Empresa e) =>
        new(e.Ruc, e.Nombre, e.DirMatriz, e.NombreComercial,
            e.ObligadoContabilidad, e.CreatedAt, e.UpdatedAt);
}

public class RegistrarEmpresaValidator : AbstractValidator<RegistrarEmpresaRequest>
{
    public RegistrarEmpresaValidator()
    {
        RuleFor(x => x.Ruc).NotEmpty().Length(13).Matches(@"^\d+$")
            .WithMessage("RUC debe tener 13 dígitos numéricos.");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DirMatriz).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CertificadoP12Base64).NotEmpty();
        RuleFor(x => x.CertPassword).NotEmpty();
    }
}

public class ActualizarCertificadoValidator : AbstractValidator<ActualizarCertificadoRequest>
{
    public ActualizarCertificadoValidator()
    {
        RuleFor(x => x.CertificadoP12Base64).NotEmpty();
        RuleFor(x => x.CertPassword).NotEmpty();
    }
}
