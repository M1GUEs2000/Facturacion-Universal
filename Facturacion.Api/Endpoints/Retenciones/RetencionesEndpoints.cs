using Facturacion.Api.Contratos.Retenciones;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.Retenciones;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Repositorios;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.Retenciones;

public static class RetencionesEndpoints
{
    public static WebApplication MapRetencionesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/retenciones")
            .WithTags("Retenciones")
            .RequireAuthorization()
            .RequireRateLimiting("emision");

        group.MapPost("/", Emitir).WithName("EmitirRetencion");
        group.MapPost("/preview", Preview).WithName("PreviewRetencion");
        group.MapPost("/{id:guid}/reintentar", Reintentar).WithName("ReintentarRetencion");
        group.MapGet("/{id:guid}/pdf", ObtenerPdf).WithName("DescargarPdfRetencion");
        group.MapGet("/{id:guid}/xml", ObtenerXml).WithName("DescargarXmlRetencion");

        return app;
    }

    private static async Task<IResult> ObtenerPdf(
        Guid id,
        [FromServices] IRetencionesRepositorio repo,
        [FromServices] ObtenerUrlDescarga useCase,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();
        var retencion = await repo.ObtenerPorIdAsync(id, ct);
        if (retencion is null) return Results.NotFound();
        var result = await useCase.EjecutarAsync(retencion, TipoArchivoDescarga.Pdf, cuentaId, ct);
        return result.Match(r => Results.Ok(r), errors => errors.ToProblemResult());
    }

    private static async Task<IResult> ObtenerXml(
        Guid id,
        [FromServices] IRetencionesRepositorio repo,
        [FromServices] ObtenerUrlDescarga useCase,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();
        var retencion = await repo.ObtenerPorIdAsync(id, ct);
        if (retencion is null) return Results.NotFound();
        var result = await useCase.EjecutarAsync(retencion, TipoArchivoDescarga.Xml, cuentaId, ct);
        return result.Match(r => Results.Ok(r), errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Preview(
        [FromBody] EmitirRetencionRequest req,
        [FromServices] GenerarPreviewPdf useCase,
        [FromServices] IValidator<EmitirRetencionRequest> validator,
        HttpContext ctx,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var infoAdicional = req.InfoAdicional
            .Select(i => new InfoAdicional(i.Nombre, i.Valor))
            .ToList();

        var detalle = req.Detalle
            .Select(d => new ComandoDetalleRetencion(
                d.Orden, d.CodigoImpuesto, d.CodigoRetencion,
                d.BaseImponible, d.PorcentajeRetener, d.ValorRetenido,
                d.CodDocSustento, d.NumDocSustento, d.FechaEmisionDocSustento))
            .ToList();

        var cmd = new ComandoEmitirRetencion(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionSujeto, req.IdentificacionSujeto,
            req.RazonSocialSujeto, req.DireccionSujeto, req.PeriodoFiscal,
            req.TotalBaseImponible, req.TotalRetencionRenta, req.TotalRetencionIva, req.TotalRetenido,
            infoAdicional, detalle, CuentaId: cuentaId);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            bytes => Results.File(bytes, "application/pdf", "preview-retencion.pdf"),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Reintentar(
        Guid id,
        [FromServices] ReintentarEmisionRetencion useCase,
        [FromServices] IAuditLogger audit,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var ip = ctx.Connection.RemoteIpAddress?.ToString();
        var result = await useCase.EjecutarAsync(id, cuentaId, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.RetencionReintentada,
            CuentaId: cuentaId,
            Ruc: result.IsError ? null : result.Value.EmpresaRuc,
            ClaveAcceso: result.IsError ? null : result.Value.ClaveAcceso,
            Ip: ip,
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
        return result.Match(
            retencion => Results.Ok(RetencionResponse.From(retencion)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Emitir(
        [FromBody] EmitirRetencionRequest req,
        [FromServices] EmitirRetencion useCase,
        [FromServices] IValidator<EmitirRetencionRequest> validator,
        [FromServices] IAuditLogger audit,
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
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var cmd = new ComandoEmitirRetencion(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionSujeto, req.IdentificacionSujeto,
            req.RazonSocialSujeto, req.DireccionSujeto, req.PeriodoFiscal,
            req.TotalBaseImponible, req.TotalRetencionRenta, req.TotalRetencionIva, req.TotalRetenido,
            infoAdicional, detalle, ip, cuentaId);

        var result = await useCase.EjecutarAsync(cmd, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.RetencionEmitida,
            CuentaId: cuentaId,
            Ruc: req.EmpresaRuc,
            ClaveAcceso: result.IsError ? null : result.Value.ClaveAcceso,
            Ip: ip,
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
        return result.Match(
            retencion => Results.Created($"/retenciones/{retencion.Id}", RetencionResponse.From(retencion)),
            errors => errors.ToProblemResult());
    }
}
