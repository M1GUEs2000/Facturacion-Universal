using System.Text;
using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.NotasCredito;

public record ComandoDetalleNotaCredito(
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

public record ComandoEmitirNotaCredito(
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
    string DocModificadoTipo,
    string DocModificadoNumero,
    DateOnly DocModificadoFecha,
    string DocModificadoClaveAcceso,
    string Motivo,
    decimal TotalSinImpuestos,
    decimal TotalDescuento,
    decimal? BaseImponibleIce,
    decimal? ValorIce,
    decimal BaseImponibleIva,
    decimal ValorIva,
    decimal ValorModificacion,
    List<InfoAdicional> InfoAdicional,
    List<ComandoDetalleNotaCredito> Detalle,
    string? IpAddress = null);

public class EmitirNotaCredito(
    IEmpresasRepositorio empresas,
    INotasCreditoRepositorio notasCredito,
    IServicioXml xml,
    IServicioFirma firma,
    IServicioSri sri,
    IServicioPdf pdf,
    IServicioStorage storage)
{
    public async Task<ErrorOr<NotaCredito>> EjecutarAsync(ComandoEmitirNotaCredito cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.EmpresaRuc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.NotaCredito, empresa.Ruc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial);

        if (await notasCredito.ExisteClaveAccesoAsync(claveAcceso, ct))
            return Errores.NotaCredito.ClaveAccesoDuplicada;

        var notaId = Guid.NewGuid();
        var detalle = cmd.Detalle.Select(d => NotaCreditoDetalle.Crear(
            notaId, d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
            d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
            d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
            d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor)).ToList();

        var nota = NotaCredito.Crear(
            cmd.EmpresaRuc, cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial, claveAcceso,
            cmd.FechaEmision, cmd.TipoIdentificacionComprador, cmd.IdentificacionComprador,
            cmd.RazonSocialComprador, cmd.DireccionComprador, cmd.DirEstablecimiento,
            cmd.DocModificadoTipo, cmd.DocModificadoNumero, cmd.DocModificadoFecha, cmd.DocModificadoClaveAcceso,
            cmd.Motivo, cmd.TotalSinImpuestos, cmd.TotalDescuento, cmd.BaseImponibleIce, cmd.ValorIce,
            cmd.BaseImponibleIva, cmd.ValorIva, cmd.ValorModificacion,
            cmd.InfoAdicional, detalle, cmd.IpAddress);

        var xmlResult = xml.GenerarXmlNotaCredito(nota, empresa);
        if (xmlResult.IsError) return xmlResult.Errors;

        var firmadoResult = await firma.FirmarXmlAsync(xmlResult.Value, empresa.CertificadoP12, empresa.CertPassword, ct);
        if (firmadoResult.IsError) return firmadoResult.Errors;

        var xmlPath = $"{empresa.Ruc}/notas-credito/{claveAcceso}.xml";
        var storageXmlResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(firmadoResult.Value), xmlPath, ct);
        if (storageXmlResult.IsError) return storageXmlResult.Errors;

        nota.RegistrarXmlFirmado(storageXmlResult.Value);

        var recepcionResult = await sri.EnviarDocumentoAsync(firmadoResult.Value, cmd.Ambiente, ct);
        if (recepcionResult.IsError) return recepcionResult.Errors;

        nota.RegistrarEnvioSri();

        var autorizacionResult = await sri.ConsultarAutorizacionAsync(claveAcceso, cmd.Ambiente, ct);
        if (autorizacionResult.IsError) return autorizacionResult.Errors;

        var auth = autorizacionResult.Value;

        if (!auth.Autorizado)
        {
            nota.RegistrarNoAutorizacion(auth.MensajeSri);
            await notasCredito.AgregarAsync(nota, ct);
            return Errores.Sri.NoAutorizado(auth.MensajeSri);
        }

        var pdfResult = await pdf.GenerarRideNotaCreditoAsync(nota, ct);
        if (pdfResult.IsError) return pdfResult.Errors;

        var pdfPath = $"{empresa.Ruc}/notas-credito/{claveAcceso}.pdf";
        var storagePdfResult = await storage.GuardarAsync(pdfResult.Value, pdfPath, ct);
        if (storagePdfResult.IsError) return storagePdfResult.Errors;

        var xmlAutPath = $"{empresa.Ruc}/notas-credito/{claveAcceso}_autorizado.xml";
        var storageXmlAutResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(auth.XmlAutorizado!), xmlAutPath, ct);
        if (storageXmlAutResult.IsError) return storageXmlAutResult.Errors;

        nota.RegistrarAutorizacion(auth.NumeroAutorizacion!, auth.FechaAutorizacion!.Value, storageXmlAutResult.Value, storagePdfResult.Value);

        await notasCredito.AgregarAsync(nota, ct);

        return nota;
    }
}
