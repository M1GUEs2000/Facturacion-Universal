using Facturacion.Api.Contratos.NotasCredito;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.NotasCredito;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Repositorios;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.NotasCredito;

public static class NotasCreditoEndpoints
{
    public static WebApplication MapNotasCreditoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/notas-credito")
            .WithTags("Notas de Crédito")
            .RequireAuthorization()
            .RequireRateLimiting("emision");

        group.MapPost("/", Emitir).WithName("EmitirNotaCredito");
        group.MapPost("/preview", Preview).WithName("PreviewNotaCredito");
        group.MapPost("/{id:guid}/reintentar", Reintentar).WithName("ReintentarNotaCredito");
        group.MapGet("/{id:guid}/pdf", ObtenerPdf).WithName("DescargarPdfNotaCredito");
        group.MapGet("/{id:guid}/xml", ObtenerXml).WithName("DescargarXmlNotaCredito");

        return app;
    }

    private static async Task<IResult> ObtenerPdf(
        Guid id,
        [FromServices] INotasCreditoRepositorio repo,
        [FromServices] ObtenerUrlDescarga useCase,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();
        var nota = await repo.ObtenerPorIdAsync(id, ct);
        if (nota is null) return Results.NotFound();
        var result = await useCase.EjecutarAsync(nota, TipoArchivoDescarga.Pdf, cuentaId, ct);
        return result.Match(r => Results.Ok(r), errors => errors.ToProblemResult());
    }

    private static async Task<IResult> ObtenerXml(
        Guid id,
        [FromServices] INotasCreditoRepositorio repo,
        [FromServices] ObtenerUrlDescarga useCase,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();
        var nota = await repo.ObtenerPorIdAsync(id, ct);
        if (nota is null) return Results.NotFound();
        var result = await useCase.EjecutarAsync(nota, TipoArchivoDescarga.Xml, cuentaId, ct);
        return result.Match(r => Results.Ok(r), errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Preview(
        [FromBody] EmitirNotaCreditoRequest req,
        [FromServices] GenerarPreviewPdf useCase,
        [FromServices] IValidator<EmitirNotaCreditoRequest> validator,
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
            .Select(d => new ComandoDetalleNotaCredito(
                d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
                d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
                d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
                d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor))
            .ToList();

        var cmd = new ComandoEmitirNotaCredito(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionComprador, req.IdentificacionComprador,
            req.RazonSocialComprador, req.DireccionComprador, req.DirEstablecimiento,
            req.DocModificadoTipo, req.DocModificadoNumero, req.DocModificadoFecha,
            req.DocModificadoClaveAcceso, req.Motivo,
            req.TotalSinImpuestos, req.TotalDescuento, req.BaseImponibleIce, req.ValorIce,
            req.BaseImponibleIva, req.ValorIva, req.ValorModificacion,
            infoAdicional, detalle, CuentaId: cuentaId);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            bytes => Results.File(bytes, "application/pdf", "preview-nota-credito.pdf"),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Reintentar(
        Guid id,
        [FromServices] ReintentarEmisionNotaCredito useCase,
        [FromServices] IAuditLogger audit,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var ip = ctx.Connection.RemoteIpAddress?.ToString();
        var result = await useCase.EjecutarAsync(id, cuentaId, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.NotaCreditoReintentada,
            CuentaId: cuentaId,
            Ruc: result.IsError ? null : result.Value.EmpresaRuc,
            ClaveAcceso: result.IsError ? null : result.Value.ClaveAcceso,
            Ip: ip,
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
        return result.Match(
            nota => Results.Ok(NotaCreditoResponse.From(nota)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Emitir(
        [FromBody] EmitirNotaCreditoRequest req,
        [FromServices] EmitirNotaCredito useCase,
        [FromServices] IValidator<EmitirNotaCreditoRequest> validator,
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
            .Select(d => new ComandoDetalleNotaCredito(
                d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
                d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
                d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
                d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor))
            .ToList();

        var ip = ctx.Connection.RemoteIpAddress?.ToString();
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var cmd = new ComandoEmitirNotaCredito(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionComprador, req.IdentificacionComprador,
            req.RazonSocialComprador, req.DireccionComprador, req.DirEstablecimiento,
            req.DocModificadoTipo, req.DocModificadoNumero, req.DocModificadoFecha,
            req.DocModificadoClaveAcceso, req.Motivo,
            req.TotalSinImpuestos, req.TotalDescuento, req.BaseImponibleIce, req.ValorIce,
            req.BaseImponibleIva, req.ValorIva, req.ValorModificacion,
            infoAdicional, detalle, ip, cuentaId);

        var result = await useCase.EjecutarAsync(cmd, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.NotaCreditoEmitida,
            CuentaId: cuentaId,
            Ruc: req.EmpresaRuc,
            ClaveAcceso: result.IsError ? null : result.Value.ClaveAcceso,
            Ip: ip,
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
        return result.Match(
            nota => Results.Created($"/notas-credito/{nota.Id}", NotaCreditoResponse.From(nota)),
            errors => errors.ToProblemResult());
    }
}
