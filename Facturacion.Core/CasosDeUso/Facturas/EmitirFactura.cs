using System.Text;
using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core;
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
    IServicioXml xml,
    IServicioFirma firma,
    IServicioSri sri,
    IServicioPdf pdf,
    IServicioStorage storage)
{
    public async Task<ErrorOr<Factura>> EjecutarAsync(ComandoEmitirFactura cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.EmpresaRuc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.Factura, empresa.Ruc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial);

        if (await facturas.ExisteClaveAccesoAsync(claveAcceso, ct))
            return Errores.Factura.ClaveAccesoDuplicada;

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

        // Necesitamos el mismo Id en la entidad que generamos arriba para el detalle
        // Se resuelve en la infraestructura al persistir — el Id de Factura y los FacturaId del detalle deben coincidir.

        var xmlResult = xml.GenerarXmlFactura(factura, empresa);
        if (xmlResult.IsError) return xmlResult.Errors;

        var firmadoResult = await firma.FirmarXmlAsync(xmlResult.Value, empresa.CertificadoP12, empresa.CertPassword, ct);
        if (firmadoResult.IsError) return firmadoResult.Errors;

        var xmlPath = $"{empresa.Ruc}/facturas/{claveAcceso}.xml";
        var storageXmlResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(firmadoResult.Value), xmlPath, ct);
        if (storageXmlResult.IsError) return storageXmlResult.Errors;

        factura.RegistrarXmlFirmado(storageXmlResult.Value);

        var recepcionResult = await sri.EnviarDocumentoAsync(firmadoResult.Value, cmd.Ambiente, ct);
        if (recepcionResult.IsError) return recepcionResult.Errors;

        factura.RegistrarEnvioSri();

        var autorizacionResult = await sri.ConsultarAutorizacionAsync(claveAcceso, cmd.Ambiente, ct);
        if (autorizacionResult.IsError) return autorizacionResult.Errors;

        var auth = autorizacionResult.Value;

        if (!auth.Autorizado)
        {
            factura.RegistrarNoAutorizacion(auth.MensajeSri);
            await facturas.AgregarAsync(factura, ct);
            return Errores.Sri.NoAutorizado(auth.MensajeSri);
        }

        var pdfResult = await pdf.GenerarRideFacturaAsync(factura, ct);
        if (pdfResult.IsError) return pdfResult.Errors;

        var pdfPath = $"{empresa.Ruc}/facturas/{claveAcceso}.pdf";
        var storagePdfResult = await storage.GuardarAsync(pdfResult.Value, pdfPath, ct);
        if (storagePdfResult.IsError) return storagePdfResult.Errors;

        var xmlAutPath = $"{empresa.Ruc}/facturas/{claveAcceso}_autorizado.xml";
        var storageXmlAutResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(auth.XmlAutorizado!), xmlAutPath, ct);
        if (storageXmlAutResult.IsError) return storageXmlAutResult.Errors;

        factura.RegistrarAutorizacion(auth.NumeroAutorizacion!, auth.FechaAutorizacion!.Value, storageXmlAutResult.Value, storagePdfResult.Value);

        await facturas.AgregarAsync(factura, ct);

        return factura;
    }
}
