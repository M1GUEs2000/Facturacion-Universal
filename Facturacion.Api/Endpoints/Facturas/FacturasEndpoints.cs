using Facturacion.Api.Contratos.Facturas;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.Facturas;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Metodos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.Facturas;

public static class FacturasEndpoints
{
    public static WebApplication MapFacturasEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/facturas")
            .WithTags("Facturas")
            .AllowAnonymous();

        group.MapPost("/", Emitir).WithName("EmitirFactura");
        group.MapPost("/preview", Preview).WithName("PreviewFactura");
        group.MapPost("/{id:guid}/reintentar", Reintentar).WithName("ReintentarFactura");

        return app;
    }

    private static async Task<IResult> Preview(
        [FromBody] EmitirFacturaRequest req,
        [FromServices] GenerarPreviewPdf useCase,
        [FromServices] IValidator<EmitirFacturaRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var claveAcceso = GeneradorClaveAcceso.Generar(
            req.FechaEmision, TipoDocumentoSri.Factura, req.EmpresaRuc,
            req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial);

        var facturaId = Guid.NewGuid();
        var detalle = req.Detalle
            .Select(d => FacturaDetalle.Crear(
                facturaId, d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
                d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
                d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
                d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor))
            .ToList();

        var factura = Factura.Crear(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial, claveAcceso,
            req.FechaEmision, req.TipoIdentificacionComprador, req.IdentificacionComprador,
            req.RazonSocialComprador, req.DireccionComprador, req.DirEstablecimiento,
            req.TotalSinImpuestos, req.TotalDescuento, req.BaseImponibleIce, req.ValorIce,
            req.BaseImponibleIva, req.ValorIva, req.Propina, req.ImporteTotal,
            req.GuiaRemision,
            req.FormasPago.Select(f => new FormaPago(f.Codigo, f.Total, f.Plazo, f.UnidadTiempo)).ToList(),
            req.InfoAdicional.Select(i => new InfoAdicional(i.Nombre, i.Valor)).ToList(),
            detalle);

        var result = await useCase.EjecutarAsync(req.EmpresaRuc, factura, ct);
        return result.Match(
            bytes => Results.File(bytes, "application/pdf", "preview-factura.pdf"),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Reintentar(
        Guid id,
        [FromServices] ReintentarEmisionFactura useCase,
        CancellationToken ct)
    {
        var result = await useCase.EjecutarAsync(id, ct);
        return result.Match(
            factura => Results.Ok(FacturaResponse.From(factura)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Emitir(
        [FromBody] EmitirFacturaRequest req,
        [FromServices] EmitirFactura useCase,
        [FromServices] IValidator<EmitirFacturaRequest> validator,
        HttpContext ctx,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var formasPago = req.FormasPago
            .Select(f => new FormaPago(f.Codigo, f.Total, f.Plazo, f.UnidadTiempo))
            .ToList();

        var infoAdicional = req.InfoAdicional
            .Select(i => new InfoAdicional(i.Nombre, i.Valor))
            .ToList();

        var detalle = req.Detalle
            .Select(d => new ComandoDetalleFactura(
                d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
                d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
                d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
                d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor))
            .ToList();

        var ip = ctx.Connection.RemoteIpAddress?.ToString();
        var cmd = new ComandoEmitirFactura(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionComprador, req.IdentificacionComprador,
            req.RazonSocialComprador, req.DireccionComprador, req.DirEstablecimiento,
            req.TotalSinImpuestos, req.TotalDescuento, req.BaseImponibleIce, req.ValorIce,
            req.BaseImponibleIva, req.ValorIva, req.Propina, req.ImporteTotal,
            req.GuiaRemision, formasPago, infoAdicional, detalle, ip);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            factura => Results.Created($"/facturas/{factura.Id}", FacturaResponse.From(factura)),
            errors => errors.ToProblemResult());
    }
}
