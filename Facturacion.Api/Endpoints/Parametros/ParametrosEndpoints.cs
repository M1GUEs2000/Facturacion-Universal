using Facturacion.Api.Contratos.Parametros;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Parametros;
using Facturacion.Core.Interfaces.Repositorios;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Facturacion.Api.Endpoints.Parametros;

public static class ParametrosEndpoints
{
    public static WebApplication MapParametrosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/parametros")
            .WithTags("Parametros")
            .RequireAuthorization()
            .RequireRateLimiting("escritura");

        group.MapGet("/{empresaRuc}/sri", ListarSri).WithName("ListarSecuencialesSri");
        group.MapPut("/{empresaRuc}/sri/{tipoComprobante}", GuardarSri).WithName("GuardarSecuencialSri");
        group.MapGet("/{empresaRuc}/facturacion", ObtenerFacturacion).WithName("ObtenerParametrosFacturacion");
        group.MapPut("/{empresaRuc}/facturacion", GuardarFacturacion).WithName("GuardarParametrosFacturacion");

        return app;
    }

    private static async Task<IResult> ListarSri(
        string empresaRuc,
        [FromServices] ISecuencialesSriRepositorio parametros,
        [FromServices] IEmpresasRepositorio empresas,
        [FromServices] ILoggerFactory loggers,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var empresa = await empresas.ObtenerPorRucAsync(empresaRuc, ct);
        if (empresa is null || empresa.CuentaId != cuentaId)
        {
            if (empresa is not null)
                loggers.CreateLogger("Facturacion.Endpoints.Parametros").LogWarning("Auth failure: cuenta {CuentaId} intentó acceder a parametros sri de empresa {Ruc}", cuentaId, empresaRuc);
            return Results.NotFound(new { error = "La empresa no existe." });
        }

        var lista = await parametros.ListarPorEmpresaAsync(empresaRuc, ct);
        return Results.Ok(lista.Select(SecuencialSriResponse.From));
    }

    private static async Task<IResult> GuardarSri(
        string empresaRuc,
        string tipoComprobante,
        [FromBody] SecuencialSriRequest req,
        [FromServices] GuardarSecuencialSri useCase,
        [FromServices] IEmpresasRepositorio empresas,
        [FromServices] IValidator<SecuencialSriRequest> validator,
        [FromServices] ILoggerFactory loggers,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var empresa = await empresas.ObtenerPorRucAsync(empresaRuc, ct);
        if (empresa is null || empresa.CuentaId != cuentaId)
        {
            if (empresa is not null)
                loggers.CreateLogger("Facturacion.Endpoints.Parametros").LogWarning("Auth failure: cuenta {CuentaId} intentó modificar parametros sri de empresa {Ruc}", cuentaId, empresaRuc);
            return Results.NotFound(new { error = "La empresa no existe." });
        }

        var request = req with { TipoComprobante = tipoComprobante };
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var result = await useCase.EjecutarAsync(
            new ComandoGuardarSecuencialSri(
                empresaRuc, tipoComprobante, request.Secuencial, request.CodigoNumerico),
            ct);

        return result.Match(
            parametro => Results.Ok(SecuencialSriResponse.From(parametro)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> ObtenerFacturacion(
        string empresaRuc,
        [FromServices] IParametrosFacturacionRepositorio parametros,
        [FromServices] IEmpresasRepositorio empresas,
        [FromServices] ILoggerFactory loggers,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var empresa = await empresas.ObtenerPorRucAsync(empresaRuc, ct);
        if (empresa is null || empresa.CuentaId != cuentaId)
        {
            if (empresa is not null)
                loggers.CreateLogger("Facturacion.Endpoints.Parametros").LogWarning("Auth failure: cuenta {CuentaId} intentó acceder a parametros facturacion de empresa {Ruc}", cuentaId, empresaRuc);
            return Results.NotFound(new { error = "La empresa no existe." });
        }

        var config = await parametros.ObtenerPorEmpresaAsync(empresaRuc, ct);
        return config is null
            ? Results.NotFound(new { error = "Los parametros de facturacion no existen." })
            : Results.Ok(ParametrosFacturacionResponse.From(config));
    }

    private static async Task<IResult> GuardarFacturacion(
        string empresaRuc,
        [FromBody] ParametrosFacturacionRequest req,
        [FromServices] GuardarParametrosFacturacion useCase,
        [FromServices] IEmpresasRepositorio empresas,
        [FromServices] IValidator<ParametrosFacturacionRequest> validator,
        [FromServices] ILoggerFactory loggers,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var empresa = await empresas.ObtenerPorRucAsync(empresaRuc, ct);
        if (empresa is null || empresa.CuentaId != cuentaId)
        {
            if (empresa is not null)
                loggers.CreateLogger("Facturacion.Endpoints.Parametros").LogWarning("Auth failure: cuenta {CuentaId} intentó modificar parametros facturacion de empresa {Ruc}", cuentaId, empresaRuc);
            return Results.NotFound(new { error = "La empresa no existe." });
        }

        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var result = await useCase.EjecutarAsync(
            new ComandoGuardarParametrosFacturacion(
                empresaRuc, req.Ambiente, req.TipoEmision, req.AgenteRetencion,
                req.ContribuyenteRimpe, req.Estab, req.PuntoEmision,
                req.ContribuyenteEspecial, req.ObligadoContabilidad, req.Moneda,
                req.CodigoImpuesto, req.CodigoPorcentaje),
            ct);

        return result.Match(
            parametros => Results.Ok(ParametrosFacturacionResponse.From(parametros)),
            errors => errors.ToProblemResult());
    }
}
