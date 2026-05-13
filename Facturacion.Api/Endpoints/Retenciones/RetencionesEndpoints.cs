using Facturacion.Api.Contratos.Retenciones;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Retenciones;
using Facturacion.Core.Entidades;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.Retenciones;

public static class RetencionesEndpoints
{
    public static WebApplication MapRetencionesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/retenciones")
            .WithTags("Retenciones")
            .RequireAuthorization();

        group.MapPost("/", Emitir).WithName("EmitirRetencion");
        group.MapPost("/{id:guid}/reintentar", Reintentar).WithName("ReintentarRetencion");

        return app;
    }

    private static async Task<IResult> Reintentar(
        Guid id,
        [FromServices] ReintentarEmisionRetencion useCase,
        CancellationToken ct)
    {
        var result = await useCase.EjecutarAsync(id, ct);
        return result.Match(
            retencion => Results.Ok(RetencionResponse.From(retencion)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Emitir(
        [FromBody] EmitirRetencionRequest req,
        [FromServices] EmitirRetencion useCase,
        [FromServices] IValidator<EmitirRetencionRequest> validator,
        HttpContext ctx,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var infoAdicional = req.InfoAdicional
            .Select(i => new InfoAdicional(i.Nombre, i.Valor))
            .ToList();

        var detalle = req.Detalle
            .Select(d => new ComandoDetalleRetencion(
                d.Orden, d.CodigoImpuesto, d.CodigoRetencion,
                d.BaseImponible, d.PorcentajeRetener, d.ValorRetenido,
                d.CodDocSustento, d.NumDocSustento, d.FechaEmisionDocSustento))
            .ToList();

        var ip = ctx.Connection.RemoteIpAddress?.ToString();
        var cmd = new ComandoEmitirRetencion(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionSujeto, req.IdentificacionSujeto,
            req.RazonSocialSujeto, req.DireccionSujeto, req.PeriodoFiscal,
            req.TotalBaseImponible, req.TotalRetencionRenta, req.TotalRetencionIva, req.TotalRetenido,
            infoAdicional, detalle, ip);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            retencion => Results.Created($"/retenciones/{retencion.Id}", RetencionResponse.From(retencion)),
            errors => errors.ToProblemResult());
    }
}
