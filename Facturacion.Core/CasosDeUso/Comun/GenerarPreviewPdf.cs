using ErrorOr;
using Facturacion.Core.CasosDeUso.Facturas;
using Facturacion.Core.CasosDeUso.NotasCredito;
using Facturacion.Core.CasosDeUso.Retenciones;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.Comun;

public class GenerarPreviewPdf(
    IEmpresasRepositorio empresas,
    IParametrosFacturacionRepositorio parametrosRepo,
    IServicioPdf pdf,
    IServicioStorageFirmaYLogo storageFirma)
{
    private record Contexto(Empresa Empresa, ParametrosFacturacion? Parametros, byte[]? LogoBytes);

    public async Task<ErrorOr<byte[]>> EjecutarAsync(string empresaRuc, Factura factura, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(empresaRuc, null, ct);
        if (ctx.IsError) return ctx.Errors;
        return await pdf.GenerarRideFacturaAsync(factura, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    public async Task<ErrorOr<byte[]>> EjecutarAsync(string empresaRuc, NotaCredito nc, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(empresaRuc, null, ct);
        if (ctx.IsError) return ctx.Errors;
        return await pdf.GenerarRideNotaCreditoAsync(nc, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    public async Task<ErrorOr<byte[]>> EjecutarAsync(string empresaRuc, Retencion retencion, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(empresaRuc, null, ct);
        if (ctx.IsError) return ctx.Errors;
        return await pdf.GenerarRideRetencionAsync(retencion, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    public async Task<ErrorOr<byte[]>> EjecutarAsync(ComandoEmitirFactura cmd, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(cmd.EmpresaRuc, cmd.CuentaId, ct);
        if (ctx.IsError) return ctx.Errors;

        var secuencial = cmd.Secuencial ?? "000000001";
        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.Factura, cmd.EmpresaRuc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, secuencial);

        var facturaId = Guid.NewGuid();
        var detalle = cmd.Detalle.Select(d => FacturaDetalle.Crear(
            facturaId, d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
            d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
            d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
            d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor)).ToList();

        var factura = Factura.Crear(
            cmd.EmpresaRuc, cmd.Ambiente, cmd.Estab, cmd.PtoEmi, secuencial, claveAcceso,
            cmd.FechaEmision, cmd.TipoIdentificacionComprador, cmd.IdentificacionComprador,
            cmd.RazonSocialComprador, cmd.DireccionComprador, cmd.DirEstablecimiento,
            cmd.TotalSinImpuestos, cmd.TotalDescuento, cmd.BaseImponibleIce, cmd.ValorIce,
            cmd.BaseImponibleIva, cmd.ValorIva, cmd.Propina, cmd.ImporteTotal,
            cmd.GuiaRemision, cmd.FormasPago, cmd.InfoAdicional, detalle,
            id: facturaId);

        return await pdf.GenerarRideFacturaAsync(factura, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    public async Task<ErrorOr<byte[]>> EjecutarAsync(ComandoEmitirNotaCredito cmd, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(cmd.EmpresaRuc, cmd.CuentaId, ct);
        if (ctx.IsError) return ctx.Errors;

        var secuencial = cmd.Secuencial ?? "000000001";
        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.NotaCredito, cmd.EmpresaRuc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, secuencial);

        var notaId = Guid.NewGuid();
        var detalle = cmd.Detalle.Select(d => NotaCreditoDetalle.Crear(
            notaId, d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
            d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
            d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
            d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor)).ToList();

        var nota = NotaCredito.Crear(
            cmd.EmpresaRuc, cmd.Ambiente, cmd.Estab, cmd.PtoEmi, secuencial, claveAcceso,
            cmd.FechaEmision, cmd.TipoIdentificacionComprador, cmd.IdentificacionComprador,
            cmd.RazonSocialComprador, cmd.DireccionComprador, cmd.DirEstablecimiento,
            cmd.DocModificadoTipo, cmd.DocModificadoNumero, cmd.DocModificadoFecha,
            cmd.DocModificadoClaveAcceso, cmd.Motivo,
            cmd.TotalSinImpuestos, cmd.TotalDescuento, cmd.BaseImponibleIce, cmd.ValorIce,
            cmd.BaseImponibleIva, cmd.ValorIva, cmd.ValorModificacion,
            cmd.InfoAdicional, detalle, id: notaId);

        return await pdf.GenerarRideNotaCreditoAsync(nota, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    public async Task<ErrorOr<byte[]>> EjecutarAsync(ComandoEmitirRetencion cmd, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(cmd.EmpresaRuc, cmd.CuentaId, ct);
        if (ctx.IsError) return ctx.Errors;

        var secuencial = cmd.Secuencial ?? "000000001";
        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.Retencion, cmd.EmpresaRuc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, secuencial);

        var retencionId = Guid.NewGuid();
        var detalle = cmd.Detalle.Select(d => RetencionDetalle.Crear(
            retencionId, d.Orden, d.CodigoImpuesto, d.CodigoRetencion,
            d.BaseImponible, d.PorcentajeRetener, d.ValorRetenido,
            d.CodDocSustento, d.NumDocSustento, d.FechaEmisionDocSustento)).ToList();

        var retencion = Retencion.Crear(
            cmd.EmpresaRuc, cmd.Ambiente, cmd.Estab, cmd.PtoEmi, secuencial, claveAcceso,
            cmd.FechaEmision, cmd.TipoIdentificacionSujeto, cmd.IdentificacionSujeto,
            cmd.RazonSocialSujeto, cmd.DireccionSujeto, cmd.PeriodoFiscal,
            cmd.TotalBaseImponible, cmd.TotalRetencionRenta, cmd.TotalRetencionIva, cmd.TotalRetenido,
            cmd.InfoAdicional, detalle, id: retencionId);

        return await pdf.GenerarRideRetencionAsync(retencion, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    private async Task<ErrorOr<Contexto>> ObtenerContextoAsync(string empresaRuc, Guid? cuentaId, CancellationToken ct)
    {
        var empresa = await empresas.ObtenerPorRucAsync(empresaRuc, ct);
        if (empresa is null) return Errores.Empresa.NoEncontrada;

        if (cuentaId.HasValue && empresa.CuentaId != cuentaId.Value)
            return Errores.Empresa.Prohibido;

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(empresaRuc, ct);

        byte[]? logoBytes = null;
        if (empresa.LogoPath is not null)
        {
            var logoResult = await storageFirma.ObtenerAsync(empresa.LogoPath, ct);
            if (!logoResult.IsError) logoBytes = logoResult.Value;
        }

        return new Contexto(empresa, parametros, logoBytes);
    }
}
