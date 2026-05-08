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

        group.MapPost("/", Registrar).WithName("RegistrarEmpresa");
        group.MapPut("/{ruc}/certificado", ActualizarCertificado).WithName("ActualizarCertificado");

        return app;
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
        catch { return Results.BadRequest(new { error = "CertificadoP12Base64 no es Base64 válido." }); }

        var cmd = new ComandoRegistrarEmpresa(
            req.Ruc, req.Nombre, req.DirMatriz,
            req.ObligadoContabilidad, certBytes, req.CertPassword, req.NombreComercial);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            empresa => Results.Created($"/empresas/{empresa.Ruc}", EmpresaResponse.From(empresa)),
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
        catch { return Results.BadRequest(new { error = "CertificadoP12Base64 no es Base64 válido." }); }

        var cmd = new ComandoActualizarCertificado(ruc, certBytes, req.CertPassword);
        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            empresa => Results.Ok(EmpresaResponse.From(empresa)),
            errors => errors.ToProblemResult());
    }
}
