using Facturacion.Api.Contratos.Empresas;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Empresas;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.Empresas;

public static class EmpresasEndpoints
{
    public static WebApplication MapEmpresasEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/empresas")
            .WithTags("Empresas");

        group.MapGet("", Listar).WithName("ListarEmpresas");
        group.MapGet("/{ruc}", ObtenerPorRuc).WithName("ObtenerEmpresaPorRuc");
        group.MapPost("/guardar", Guardar).WithName("GuardarEmpresa");
        group.MapPost("", Registrar).WithName("RegistrarEmpresa");
        group.MapPut("/{ruc}", Actualizar).WithName("ActualizarEmpresa");
        group.MapPut("/{ruc}/certificado", ActualizarCertificado).WithName("ActualizarCertificado");

        return app;
    }

    private static async Task<IResult> Listar(
        [FromServices] Facturacion.Core.Interfaces.Repositorios.IEmpresasRepositorio empresas,
        CancellationToken ct)
    {
        var lista = await empresas.ListarAsync(ct);
        return Results.Ok(lista.Select(EmpresaResponse.From));
    }

    private static async Task<IResult> Guardar(
        [FromBody] GuardarEmpresaRequest req,
        [FromServices] GuardarEmpresa useCase,
        [FromServices] IValidator<GuardarEmpresaRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var certResult = DecodificarBase64Opcional(req.CertificadoP12Base64, "CertificadoP12Base64");
        if (certResult.Error is not null)
            return Results.BadRequest(new { error = certResult.Error });

        var logoResult = DecodificarLogo(req.LogoBase64);
        if (logoResult.Error is not null)
            return Results.BadRequest(new { error = logoResult.Error });

        var cmd = new ComandoGuardarEmpresa(
            req.Ruc, req.Nombre, req.DirMatriz,
            req.NombreComercial, certResult.Bytes, req.CertPassword,
            logoResult.Logo, logoResult.Logo is null ? null : req.LogoContentType);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            empresa => Results.Ok(EmpresaResponse.From(empresa)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> ObtenerPorRuc(
        string ruc,
        [FromServices] Facturacion.Core.Interfaces.Repositorios.IEmpresasRepositorio empresas,
        CancellationToken ct)
    {
        var empresa = await empresas.ObtenerPorRucAsync(ruc, ct);
        return empresa is null
            ? Results.NotFound(new { error = "La empresa no existe." })
            : Results.Ok(EmpresaResponse.From(empresa));
    }

    private static async Task<IResult> Registrar(
        [FromBody] RegistrarEmpresaRequest req,
        [FromServices] RegistrarEmpresa useCase,
        [FromServices] IValidator<RegistrarEmpresaRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        byte[] certBytes;
        try { certBytes = Convert.FromBase64String(req.CertificadoP12Base64); }
        catch { return Results.BadRequest(new { error = "CertificadoP12Base64 no es Base64 valido." }); }

        var logoResult = DecodificarLogo(req.LogoBase64);
        if (logoResult.Error is not null)
            return Results.BadRequest(new { error = logoResult.Error });

        var cmd = new ComandoRegistrarEmpresa(
            req.Ruc, req.Nombre, req.DirMatriz,
            certBytes, req.CertPassword, req.NombreComercial,
            logoResult.Logo, logoResult.Logo is null ? null : req.LogoContentType);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            empresa => Results.Created($"/empresas/{empresa.Ruc}", EmpresaResponse.From(empresa)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Actualizar(
        string ruc,
        [FromBody] ActualizarEmpresaRequest req,
        [FromServices] ActualizarEmpresa useCase,
        [FromServices] IValidator<ActualizarEmpresaRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        byte[]? certBytes = null;
        if (!string.IsNullOrWhiteSpace(req.CertificadoP12Base64))
        {
            try { certBytes = Convert.FromBase64String(req.CertificadoP12Base64); }
            catch { return Results.BadRequest(new { error = "CertificadoP12Base64 no es Base64 valido." }); }
        }

        var logoResult = DecodificarLogo(req.LogoBase64);
        if (logoResult.Error is not null)
            return Results.BadRequest(new { error = logoResult.Error });

        var cmd = new ComandoActualizarEmpresa(
            ruc, req.Nombre, req.DirMatriz,
            req.NombreComercial, certBytes, req.CertPassword,
            logoResult.Logo, logoResult.Logo is null ? null : req.LogoContentType);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            empresa => Results.Ok(EmpresaResponse.From(empresa)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> ActualizarCertificado(
        string ruc,
        [FromBody] ActualizarCertificadoRequest req,
        [FromServices] ActualizarCertificado useCase,
        [FromServices] IValidator<ActualizarCertificadoRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        byte[] certBytes;
        try { certBytes = Convert.FromBase64String(req.CertificadoP12Base64); }
        catch { return Results.BadRequest(new { error = "CertificadoP12Base64 no es Base64 valido." }); }

        var cmd = new ComandoActualizarCertificado(ruc, certBytes, req.CertPassword);
        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            empresa => Results.Ok(EmpresaResponse.From(empresa)),
            errors => errors.ToProblemResult());
    }

    private static (byte[]? Logo, string? Error) DecodificarLogo(string? logoBase64)
    {
        if (string.IsNullOrWhiteSpace(logoBase64))
            return (null, null);

        byte[] logoBytes;
        try { logoBytes = Convert.FromBase64String(logoBase64); }
        catch { return (null, "LogoBase64 no es Base64 valido."); }

        return logoBytes.Length > EmpresaRequestValidation.LogoMaxBytes
            ? (null, "El logo no puede superar 2 MB.")
            : (logoBytes, null);
    }

    private static (byte[]? Bytes, string? Error) DecodificarBase64Opcional(string? base64, string campo)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return (null, null);

        try { return (Convert.FromBase64String(base64), null); }
        catch { return (null, $"{campo} no es Base64 valido."); }
    }
}
