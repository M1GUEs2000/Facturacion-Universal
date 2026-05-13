using ErrorOr;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.Facturas;

public record ComandoDetalleFactura(
    int Orden,
    string CodigoPrincipal,
    string? CodigoAuxiliar,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Descuento,
    decimal PrecioTotalSinImpuesto,
    string? IceCodigo,
    decimal? IceTarifa,
    decimal? IceBase,
    decimal? IceValor,
    CodigoIva IvaCodigo,
    decimal IvaTarifa,
    decimal IvaBase,
    decimal IvaValor);

public record ComandoEmitirFactura(
    string EmpresaRuc,
    Ambiente Ambiente,
    string Estab,
    string PtoEmi,
    string Secuencial,
    DateOnly FechaEmision,
    string TipoIdentificacionComprador,
    string IdentificacionComprador,
    string RazonSocialComprador,
    string? DireccionComprador,
    string? DirEstablecimiento,
    decimal TotalSinImpuestos,
    decimal TotalDescuento,
    decimal? BaseImponibleIce,
    decimal? ValorIce,
    decimal BaseImponibleIva,
    decimal ValorIva,
    decimal Propina,
    decimal ImporteTotal,
    string? GuiaRemision,
    List<FormaPago> FormasPago,
    List<InfoAdicional> InfoAdicional,
    List<ComandoDetalleFactura> Detalle,
    string? IpAddress = null);

public class EmitirFactura(
    IEmpresasRepositorio empresas,
    IFacturasRepositorio facturas,
    IParametrosFacturacionRepositorio parametrosRepo,
    ISecuencialesSriRepositorio secuenciales,
    IServicioXml xml,
    IServicioPdf pdf,
    IServicioStorageFirmaYLogo storageFirma,
    OrquestadorEmision orquestador)
{
    public async Task<ErrorOr<Factura>> EjecutarAsync(ComandoEmitirFactura cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.EmpresaRuc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        var certResult = await storageFirma.ObtenerAsync(empresa.CertificadoPath, ct);
        if (certResult.IsError) return certResult.Errors;

        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.Factura, empresa.Ruc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial);

        if (await facturas.ExisteSecuencialActivoAsync(empresa.Ruc, cmd.Estab, cmd.PtoEmi, cmd.Secuencial, cmd.Ambiente, ct))
            return Errores.Factura.SecuencialDuplicado;

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(cmd.EmpresaRuc, ct);

        var facturaId = Guid.NewGuid();
        var detalle = cmd.Detalle.Select(d => FacturaDetalle.Crear(
            facturaId, d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
            d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
            d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
            d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor)).ToList();

        var factura = Factura.Crear(
            cmd.EmpresaRuc, cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial, claveAcceso,
            cmd.FechaEmision, cmd.TipoIdentificacionComprador, cmd.IdentificacionComprador,
            cmd.RazonSocialComprador, cmd.DireccionComprador, cmd.DirEstablecimiento,
            cmd.TotalSinImpuestos, cmd.TotalDescuento, cmd.BaseImponibleIce, cmd.ValorIce,
            cmd.BaseImponibleIva, cmd.ValorIva, cmd.Propina, cmd.ImporteTotal,
            cmd.GuiaRemision, cmd.FormasPago, cmd.InfoAdicional, detalle, cmd.IpAddress);

        var xmlResult = xml.GenerarXmlFactura(factura, empresa, parametros);
        if (xmlResult.IsError) return xmlResult.Errors;

        await facturas.AgregarAsync(factura, ct);

        return await orquestador.EjecutarAsync(new ParametrosEmision<Factura>(
            factura, claveAcceso, cmd.Ambiente, xmlResult.Value,
            $"{empresa.Ruc}/facturas",
            certResult.Value, empresa.CertPassword,
            (f, t) => pdf.GenerarRideFacturaAsync(f, t),
            (f, t) => facturas.ActualizarAsync(f, t),
            async t =>
            {
                var sec = await secuenciales.ObtenerAsync(cmd.EmpresaRuc, "01", t);
                if (sec is not null) { sec.Incrementar(); await secuenciales.ActualizarAsync(sec, t); }
            }), ct);
    }
}
