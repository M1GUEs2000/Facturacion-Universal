using ErrorOr;
using Facturacion.Api.Contratos.Comun;
using Facturacion.Api.Contratos.Empresas;
using Facturacion.Api.Extensions;
using Facturacion.Core;
using Facturacion.Core.CasosDeUso.Empresas;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Repositorios;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.Empresas;

public static class EmpresasEndpoints
{
    public static IEndpointRouteBuilder MapEmpresasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/empresas")
            .WithTags("Empresas")
            .RequireAuthorization()
            .RequireRateLimiting("escritura");

        group.MapGet("", Listar).WithName("ListarEmpresas");
        group.MapGet("/{ruc}", ObtenerPorRuc).WithName("ObtenerEmpresaPorRuc");
        group.MapPut("/{ruc}", Actualizar).WithName("ActualizarEmpresa");
        group.MapPut("/{ruc}/certificado", ActualizarCertificado).WithName("ActualizarCertificado");

        return app;
    }

    private static async Task<IResult> Listar(
        [FromServices] IEmpresasRepositorio empresas,
        HttpContext ctx,
        CancellationToken ct,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 50)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        if (pagina < 1) pagina = 1;
        if (tamanoPagina is < 1 or > 100) tamanoPagina = 50;

        var lista = await empresas.ListarPorCuentaAsync(cuentaId, pagina, tamanoPagina, ct);
        var total = await empresas.ContarPorCuentaAsync(cuentaId, ct);

        var data = lista.Select(EmpresaResponse.From).ToList();
        return Results.Ok(new PaginaResponse<EmpresaResponse>(data, total, pagina, tamanoPagina, pagina * tamanoPagina < total));
    }

    private static async Task<IResult> ObtenerPorRuc(
        string ruc,
        [FromServices] IEmpresasRepositorio empresas,
        [FromServices] ILoggerFactory loggers,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var empresa = await empresas.ObtenerPorRucAsync(ruc, ct);
        if (empresa is null || empresa.CuentaId != cuentaId)
        {
            if (empresa is not null)
                loggers.CreateLogger("Facturacion.Endpoints.Empresas")
                    .LogWarning("Auth failure: cuenta {CuentaId} intentó acceder a empresa {Ruc}", cuentaId, ruc);
            return new List<Error> { Errores.Empresa.NoEncontrada }.ToProblemResult();
        }

        return Results.Ok(EmpresaResponse.From(empresa));
    }

    private static async Task<IResult> Actualizar(
        string ruc,
        [FromBody] ActualizarEmpresaRequest req,
        [FromServices] GuardarEmpresa useCase,
        [FromServices] IValidator<ActualizarEmpresaRequest> validator,
        [FromServices] IAuditLogger audit,
        HttpContext ctx,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var certResult = DecodificarBase64Opcional(req.CertificadoP12Base64, "CertificadoP12Base64");
        if (certResult.Error is not null)
            return Results.BadRequest(new { error = certResult.Error });

        var logoResult = DecodificarLogo(req.LogoBase64);
        if (logoResult.Error is not null)
            return Results.BadRequest(new { error = logoResult.Error });

        var cmd = new ComandoGuardarEmpresa(
            ruc, req.Nombre, req.DirMatriz, cuentaId,
            req.NombreComercial, certResult.Bytes, req.CertPassword,
            logoResult.Logo, logoResult.Logo is null ? null : req.LogoContentType);

        var result = await useCase.EjecutarAsync(cmd, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.EmpresaActualizada,
            CuentaId: cuentaId,
            Ruc: ruc,
            Ip: ctx.Connection.RemoteIpAddress?.ToString(),
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
        return result.Match(
            empresa => Results.Ok(EmpresaResponse.From(empresa)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> ActualizarCertificado(
        string ruc,
        [FromBody] ActualizarCertificadoRequest req,
        [FromServices] ActualizarCertificado useCase,
        [FromServices] IValidator<ActualizarCertificadoRequest> validator,
        [FromServices] IAuditLogger audit,
        HttpContext ctx,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        byte[] certBytes;
        try { certBytes = Convert.FromBase64String(req.CertificadoP12Base64); }
        catch { return Results.BadRequest(new { error = "CertificadoP12Base64 no es Base64 valido." }); }

        var cmd = new ComandoActualizarCertificado(ruc, certBytes, req.CertPassword, cuentaId);
        var result = await useCase.EjecutarAsync(cmd, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.CertificadoActualizado,
            CuentaId: cuentaId,
            Ruc: ruc,
            Ip: ctx.Connection.RemoteIpAddress?.ToString(),
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
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

        if (logoBytes.Length > EmpresaRequestValidation.LogoMaxBytes)
            return (null, "El logo no puede superar 2 MB.");

        if (!EsImagenValida(logoBytes))
            return (null, "El logo debe ser una imagen PNG, JPEG o WebP válida.");

        return (logoBytes, null);
    }

    private static bool EsImagenValida(byte[] bytes)
    {
        if (bytes.Length < 12) return false;
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;
        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return true;
        // WebP: RIFF????WEBP
        if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50) return true;
        return false;
    }

    private static (byte[]? Bytes, string? Error) DecodificarBase64Opcional(string? base64, string campo)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return (null, null);

        try { return (Convert.FromBase64String(base64), null); }
        catch { return (null, $"{campo} no es Base64 valido."); }
    }
}
