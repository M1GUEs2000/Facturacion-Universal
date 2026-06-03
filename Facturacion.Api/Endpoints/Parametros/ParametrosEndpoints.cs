using Facturacion.Api.Contratos.Parametros;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Parametros;
using Facturacion.Core.Interfaces.Repositorios;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.Parametros;

public static class ParametrosEndpoints
{
    public static WebApplication MapParametrosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/parametros")
            .WithTags("Parametros")
            .RequireAuthorization();

        group.MapGet("/{empresaRuc}/sri", ListarSri).WithName("ListarSecuencialesSri");
        group.MapPut("/{empresaRuc}/sri/{tipoComprobante}", GuardarSri).WithName("GuardarSecuencialSri");
        group.MapGet("/{empresaRuc}/facturacion", ObtenerFacturacion).WithName("ObtenerParametrosFacturacion");
        group.MapPut("/{empresaRuc}/facturacion", GuardarFacturacion).WithName("GuardarParametrosFacturacion");

        return app;
    }

    private static async Task<IResult> ListarSri(
        string empresaRuc,
        [FromServices] ISecuencialesSriRepositorio parametros,
        CancellationToken ct)
    {
        var lista = await parametros.ListarPorEmpresaAsync(empresaRuc, ct);
        return Results.Ok(lista.Select(SecuencialSriResponse.From));
    }

    private static async Task<IResult> GuardarSri(
        string empresaRuc,
        string tipoComprobante,
        [FromBody] SecuencialSriRequest req,
        [FromServices] GuardarSecuencialSri useCase,
        [FromServices] IValidator<SecuencialSriRequest> validator,
        CancellationToken ct)
    {
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
        CancellationToken ct)
    {
        var config = await parametros.ObtenerPorEmpresaAsync(empresaRuc, ct);
        return config is null
            ? Results.NotFound(new { error = "Los parametros de facturacion no existen." })
            : Results.Ok(ParametrosFacturacionResponse.From(config));
    }

    private static async Task<IResult> GuardarFacturacion(
        string empresaRuc,
        [FromBody] ParametrosFacturacionRequest req,
        [FromServices] GuardarParametrosFacturacion useCase,
        [FromServices] IValidator<ParametrosFacturacionRequest> validator,
        CancellationToken ct)
    {
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
