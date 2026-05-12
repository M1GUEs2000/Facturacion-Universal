using Facturacion.Core.Entidades;
using FluentValidation;

namespace Facturacion.Api.Contratos.Empresas;

public record RegistrarEmpresaRequest(
    string Ruc,
    string Nombre,
    string DirMatriz,
    string CertificadoP12Base64,
    string CertPassword,
    string? NombreComercial = null,
    string? LogoBase64 = null,
    string? LogoContentType = null);

public record GuardarEmpresaRequest(
    string Ruc,
    string Nombre,
    string DirMatriz,
    string? NombreComercial = null,
    string? CertificadoP12Base64 = null,
    string? CertPassword = null,
    string? LogoBase64 = null,
    string? LogoContentType = null);

public record ActualizarCertificadoRequest(
    string CertificadoP12Base64,
    string CertPassword);

public record ActualizarEmpresaRequest(
    string Nombre,
    string DirMatriz,
    string? NombreComercial = null,
    string? CertificadoP12Base64 = null,
    string? CertPassword = null,
    string? LogoBase64 = null,
    string? LogoContentType = null);

public record CuentaInfo(
    Guid Id,
    string Plan,
    int MaxEmpresas,
    int MaxUsuarios,
    DateTimeOffset? FechaExpira);

public record EmpresaResponse(
    string Ruc,
    string Nombre,
    string DirMatriz,
    string? NombreComercial,
    bool TieneLogo,
    string? LogoContentType,
    CuentaInfo? Cuenta,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static EmpresaResponse From(Empresa e) =>
        new(e.Ruc, e.Nombre, e.DirMatriz, e.NombreComercial,
            e.Logo is { Length: > 0 }, e.LogoContentType,
            e.Cuenta is not null
                ? new CuentaInfo(e.Cuenta.Id, e.Cuenta.Plan, e.Cuenta.MaxEmpresas, e.Cuenta.MaxUsuarios, e.Cuenta.FechaExpira)
                : null,
            e.CreatedAt, e.UpdatedAt);
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
        RuleFor(x => x.LogoContentType)
            .Must(EmpresaRequestValidation.EsLogoContentTypeValido)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoBase64))
            .WithMessage("LogoContentType debe ser image/png, image/jpeg, image/webp o image/svg+xml.");
    }
}

public class GuardarEmpresaValidator : AbstractValidator<GuardarEmpresaRequest>
{
    public GuardarEmpresaValidator()
    {
        RuleFor(x => x.Ruc).NotEmpty().Length(13).Matches(@"^\d+$")
            .WithMessage("RUC debe tener 13 digitos numericos.");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DirMatriz).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CertPassword)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.CertificadoP12Base64));
        RuleFor(x => x.LogoContentType)
            .Must(EmpresaRequestValidation.EsLogoContentTypeValido)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoBase64))
            .WithMessage("LogoContentType debe ser image/png, image/jpeg, image/webp o image/svg+xml.");
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

public class ActualizarEmpresaValidator : AbstractValidator<ActualizarEmpresaRequest>
{
    public ActualizarEmpresaValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DirMatriz).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CertPassword)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.CertificadoP12Base64));
        RuleFor(x => x.LogoContentType)
            .Must(EmpresaRequestValidation.EsLogoContentTypeValido)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoBase64))
            .WithMessage("LogoContentType debe ser image/png, image/jpeg, image/webp o image/svg+xml.");
    }
}

internal static class EmpresaRequestValidation
{
    internal const int LogoMaxBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> LogoContentTypesPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp",
        "image/svg+xml"
    };

    internal static bool EsLogoContentTypeValido(string? contentType) =>
        !string.IsNullOrWhiteSpace(contentType)
        && LogoContentTypesPermitidos.Contains(contentType);
}
