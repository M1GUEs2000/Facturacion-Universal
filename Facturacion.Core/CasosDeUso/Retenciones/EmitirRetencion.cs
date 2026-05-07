using System.Text;
using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.Retenciones;

public record ComandoDetalleRetencion(
    int Orden,
    string CodigoImpuesto,
    string CodigoRetencion,
    decimal BaseImponible,
    decimal PorcentajeRetener,
    decimal ValorRetenido,
    string CodDocSustento,
    string NumDocSustento,
    DateOnly FechaEmisionDocSustento);

public record ComandoEmitirRetencion(
    string EmpresaRuc,
    Ambiente Ambiente,
    string Estab,
    string PtoEmi,
    string Secuencial,
    DateOnly FechaEmision,
    string TipoIdentificacionSujeto,
    string IdentificacionSujeto,
    string RazonSocialSujeto,
    string? DireccionSujeto,
    string PeriodoFiscal,
    decimal TotalBaseImponible,
    decimal TotalRetencionRenta,
    decimal TotalRetencionIva,
    decimal TotalRetenido,
    List<InfoAdicional> InfoAdicional,
    List<ComandoDetalleRetencion> Detalle,
    string? IpAddress = null);

public class EmitirRetencion(
    IEmpresasRepositorio empresas,
    IRetencionesRepositorio retenciones,
    IServicioXml xml,
    IServicioFirma firma,
    IServicioSri sri,
    IServicioPdf pdf,
    IServicioStorage storage)
{
    public async Task<ErrorOr<Retencion>> EjecutarAsync(ComandoEmitirRetencion cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.EmpresaRuc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.Retencion, empresa.Ruc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial);

        if (await retenciones.ExisteClaveAccesoAsync(claveAcceso, ct))
            return Errores.Retencion.ClaveAccesoDuplicada;

        var retencionId = Guid.NewGuid();
        var detalle = cmd.Detalle.Select(d => RetencionDetalle.Crear(
            retencionId, d.Orden, d.CodigoImpuesto, d.CodigoRetencion,
            d.BaseImponible, d.PorcentajeRetener, d.ValorRetenido,
            d.CodDocSustento, d.NumDocSustento, d.FechaEmisionDocSustento)).ToList();

        var retencion = Retencion.Crear(
            cmd.EmpresaRuc, cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial, claveAcceso,
            cmd.FechaEmision, cmd.TipoIdentificacionSujeto, cmd.IdentificacionSujeto,
            cmd.RazonSocialSujeto, cmd.DireccionSujeto, cmd.PeriodoFiscal,
            cmd.TotalBaseImponible, cmd.TotalRetencionRenta, cmd.TotalRetencionIva, cmd.TotalRetenido,
            cmd.InfoAdicional, detalle, cmd.IpAddress);

        var xmlResult = xml.GenerarXmlRetencion(retencion, empresa);
        if (xmlResult.IsError) return xmlResult.Errors;

        var firmadoResult = await firma.FirmarXmlAsync(xmlResult.Value, empresa.CertificadoP12, empresa.CertPassword, ct);
        if (firmadoResult.IsError) return firmadoResult.Errors;

        var xmlPath = $"{empresa.Ruc}/retenciones/{claveAcceso}.xml";
        var storageXmlResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(firmadoResult.Value), xmlPath, ct);
        if (storageXmlResult.IsError) return storageXmlResult.Errors;

        retencion.RegistrarXmlFirmado(storageXmlResult.Value);

        var recepcionResult = await sri.EnviarDocumentoAsync(firmadoResult.Value, cmd.Ambiente, ct);
        if (recepcionResult.IsError) return recepcionResult.Errors;

        retencion.RegistrarEnvioSri();

        var autorizacionResult = await sri.ConsultarAutorizacionAsync(claveAcceso, cmd.Ambiente, ct);
        if (autorizacionResult.IsError) return autorizacionResult.Errors;

        var auth = autorizacionResult.Value;

        if (!auth.Autorizado)
        {
            retencion.RegistrarNoAutorizacion(auth.MensajeSri);
            await retenciones.AgregarAsync(retencion, ct);
            return Errores.Sri.NoAutorizado(auth.MensajeSri);
        }

        var pdfResult = await pdf.GenerarRideRetencionAsync(retencion, ct);
        if (pdfResult.IsError) return pdfResult.Errors;

        var pdfPath = $"{empresa.Ruc}/retenciones/{claveAcceso}.pdf";
        var storagePdfResult = await storage.GuardarAsync(pdfResult.Value, pdfPath, ct);
        if (storagePdfResult.IsError) return storagePdfResult.Errors;

        var xmlAutPath = $"{empresa.Ruc}/retenciones/{claveAcceso}_autorizado.xml";
        var storageXmlAutResult = await storage.GuardarAsync(Encoding.UTF8.GetBytes(auth.XmlAutorizado!), xmlAutPath, ct);
        if (storageXmlAutResult.IsError) return storageXmlAutResult.Errors;

        retencion.RegistrarAutorizacion(auth.NumeroAutorizacion!, auth.FechaAutorizacion!.Value, storageXmlAutResult.Value, storagePdfResult.Value);

        await retenciones.AgregarAsync(retencion, ct);

        return retencion;
    }
}
