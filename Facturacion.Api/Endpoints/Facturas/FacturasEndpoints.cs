using Facturacion.Api.Contratos.Comun;
using Facturacion.Api.Contratos.Facturas;
using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.Facturas;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Repositorios;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.Facturas;

public static class FacturasEndpoints
{
    public static IEndpointRouteBuilder MapFacturasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/facturas")
            .WithTags("Facturas")
            .RequireAuthorization()
            .RequireRateLimiting("emision");

        group.MapGet("", Listar).WithName("ListarFacturas");
        group.MapPost("/", Emitir).WithName("EmitirFactura");
        group.MapPost("/preview", Preview).WithName("PreviewFactura");
        group.MapPost("/{id:guid}/reintentar", Reintentar).WithName("ReintentarFactura");
        group.MapGet("/{id:guid}/pdf", ObtenerPdf).WithName("DescargarPdfFactura");
        group.MapGet("/{id:guid}/xml", ObtenerXml).WithName("DescargarXmlFactura");

        return app;
    }

    private static async Task<IResult> Listar(
        [FromServices] IFacturasRepositorio facturas,
        [FromServices] IEmpresasRepositorio empresas,
        HttpContext ctx,
        CancellationToken ct,
        [FromQuery] string empresaRuc = "",
        [FromQuery] EstadoSri? estado = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 50)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(empresaRuc))
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["empresaRuc"] = ["El parámetro empresaRuc es requerido."] });

        var empresa = await empresas.ObtenerPorRucAsync(empresaRuc, ct);
        if (empresa is null || empresa.CuentaId != cuentaId)
            return Results.NotFound();

        if (pagina < 1) pagina = 1;
        if (tamanoPagina is < 1 or > 100) tamanoPagina = 50;

        var lista = await facturas.ListarPorEmpresaAsync(empresaRuc, estado, pagina, tamanoPagina, ct);
        var total = await facturas.ContarPorEmpresaAsync(empresaRuc, estado, ct);
        var data = lista.Select(FacturaResponse.From).ToList();
        return Results.Ok(new PaginaResponse<FacturaResponse>(data, total, pagina, tamanoPagina, pagina * tamanoPagina < total));
    }

    private static async Task<IResult> ObtenerPdf(
        Guid id,
        [FromServices] IFacturasRepositorio repo,
        [FromServices] ObtenerUrlDescarga useCase,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();
        var factura = await repo.ObtenerPorIdAsync(id, ct);
        if (factura is null) return Results.NotFound();
        var result = await useCase.EjecutarAsync(factura, TipoArchivoDescarga.Pdf, cuentaId, ct);
        return result.Match(r => Results.Ok(r), errors => errors.ToProblemResult());
    }

    private static async Task<IResult> ObtenerXml(
        Guid id,
        [FromServices] IFacturasRepositorio repo,
        [FromServices] ObtenerUrlDescarga useCase,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();
        var factura = await repo.ObtenerPorIdAsync(id, ct);
        if (factura is null) return Results.NotFound();
        var result = await useCase.EjecutarAsync(factura, TipoArchivoDescarga.Xml, cuentaId, ct);
        return result.Match(r => Results.Ok(r), errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Preview(
        [FromBody] EmitirFacturaRequest req,
        [FromServices] GenerarPreviewPdf useCase,
        [FromServices] IValidator<EmitirFacturaRequest> validator,
        HttpContext ctx,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

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

        var cmd = new ComandoEmitirFactura(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionComprador, req.IdentificacionComprador,
            req.RazonSocialComprador, req.DireccionComprador, req.DirEstablecimiento,
            req.TotalSinImpuestos, req.TotalDescuento, req.BaseImponibleIce, req.ValorIce,
            req.BaseImponibleIva, req.ValorIva, req.Propina, req.ImporteTotal,
            req.GuiaRemision, formasPago, infoAdicional, detalle, CuentaId: cuentaId);

        var result = await useCase.EjecutarAsync(cmd, ct);
        return result.Match(
            bytes => Results.File(bytes, "application/pdf", "preview-factura.pdf"),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Reintentar(
        Guid id,
        [FromServices] ReintentarEmisionFactura useCase,
        [FromServices] IAuditLogger audit,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var ip = ctx.Connection.RemoteIpAddress?.ToString();
        var result = await useCase.EjecutarAsync(id, cuentaId, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.FacturaReintentada,
            CuentaId: cuentaId,
            Ruc: result.IsError ? null : result.Value.EmpresaRuc,
            ClaveAcceso: result.IsError ? null : result.Value.ClaveAcceso,
            Ip: ip,
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
        return result.Match(
            factura => Results.Ok(FacturaResponse.From(factura)),
            errors => errors.ToProblemResult());
    }

    private static async Task<IResult> Emitir(
        [FromBody] EmitirFacturaRequest req,
        [FromServices] EmitirFactura useCase,
        [FromServices] IValidator<EmitirFacturaRequest> validator,
        [FromServices] IAuditLogger audit,
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
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaId))
            return Results.Unauthorized();

        var cmd = new ComandoEmitirFactura(
            req.EmpresaRuc, req.Ambiente, req.Estab, req.PtoEmi, req.Secuencial,
            req.FechaEmision, req.TipoIdentificacionComprador, req.IdentificacionComprador,
            req.RazonSocialComprador, req.DireccionComprador, req.DirEstablecimiento,
            req.TotalSinImpuestos, req.TotalDescuento, req.BaseImponibleIce, req.ValorIce,
            req.BaseImponibleIva, req.ValorIva, req.Propina, req.ImporteTotal,
            req.GuiaRemision, formasPago, infoAdicional, detalle, ip, cuentaId);

        var result = await useCase.EjecutarAsync(cmd, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.FacturaEmitida,
            CuentaId: cuentaId,
            Ruc: req.EmpresaRuc,
            ClaveAcceso: result.IsError ? null : result.Value.ClaveAcceso,
            Ip: ip,
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
        return result.Match(
            factura => Results.Created($"/facturas/{factura.Id}", FacturaResponse.From(factura)),
            errors => errors.ToProblemResult());
    }
}
