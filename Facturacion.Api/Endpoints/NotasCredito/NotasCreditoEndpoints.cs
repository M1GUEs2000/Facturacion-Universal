using Facturacion.Api.Contratos.NotasCredito;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.NotasCredito;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Metodos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.NotasCredito;

public static class NotasCreditoEndpoints
{
    public static WebApplication MapNotasCreditoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/notas-credito")
            .WithTags("Notas de Crédito")
            .RequireAuthorization();

        group.MapPost("/", Emitir).WithName("EmitirNotaCredito");
        group.MapPost("/preview", Preview).WithName("PreviewNotaCredito");
        group.MapPost("/{id:guid}/reintentar", Reintentar).WithName("ReintentarNotaCredito");

        return app;
    }

    private static async Task<IResult> Preview(
        [FromBody] EmitirNotaCreditoRequest req,
        [FromServices] GenerarPreviewPdf useCase,
        [FromServices] IValidator<EmitirNotaCreditoRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var claveAcceso = GeneradorClaveAcceso.Generar(
            req.FechaEmision, TipoDocumentoSri.NotaCredito, req.EmpresaRuc,
            req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial);

        var notaId = Guid.NewGuid();
        var detalle = req.Detalle
            .Select(d => NotaCreditoDetalle.Crear(
                notaId, d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
                d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
                d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
                d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor))
            .ToList();

        var nota = NotaCredito.Crear(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial, claveAcceso,
            req.FechaEmision, req.TipoIdentificacionComprador, req.IdentificacionComprador,
            req.RazonSocialComprador, req.DireccionComprador, req.DirEstablecimiento,
            req.DocModificadoTipo, req.DocModificadoNumero, req.DocModificadoFecha,
            req.DocModificadoClaveAcceso, req.Motivo,
            req.TotalSinImpuestos, req.TotalDescuento, req.BaseImponibleIce, req.ValorIce,
            req.BaseImponibleIva, req.ValorIva, req.ValorModificacion,
            req.InfoAdicional.Select(i => new InfoAdicional(i.Nombre, i.Valor)).ToList(),
            detalle);

        var result = await useCase.EjecutarAsync(req.EmpresaRuc, nota, ct);
        return result.Match(
            bytes => Results.File(bytes, "application/pdf", "preview-nota-credito.pdf"),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Reintentar(
        Guid id,
        [FromServices] ReintentarEmisionNotaCredito useCase,
        CancellationToken ct)
    {
        var result = await useCase.EjecutarAsync(id, ct);
        return result.Match(
            nota => Results.Ok(NotaCreditoResponse.From(nota)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Emitir(
        [FromBody] EmitirNotaCreditoRequest req,
        [FromServices] EmitirNotaCredito useCase,
        [FromServices] IValidator<EmitirNotaCreditoRequest> validator,
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
        var cmd = new ComandoEmitirNotaCredito(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionComprador, req.IdentificacionComprador,
            req.RazonSocialComprador, req.DireccionComprador, req.DirEstablecimiento,
            req.DocModificadoTipo, req.DocModificadoNumero, req.DocModificadoFecha,
            req.DocModificadoClaveAcceso, req.Motivo,
            req.TotalSinImpuestos, req.TotalDescuento, req.BaseImponibleIce, req.ValorIce,
            req.BaseImponibleIva, req.ValorIva, req.ValorModificacion,
            infoAdicional, detalle, ip);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            nota => Results.Created($"/notas-credito/{nota.Id}", NotaCreditoResponse.From(nota)),
            errors => errors.ToProblemResult());
    }
}
