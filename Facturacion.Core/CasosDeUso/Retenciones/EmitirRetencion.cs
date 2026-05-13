using ErrorOr;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
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
    IParametrosFacturacionRepositorio parametrosRepo,
    ISecuencialesSriRepositorio secuenciales,
    IServicioXml xml,
    IServicioPdf pdf,
    OrquestadorEmision orquestador)
{
    public async Task<ErrorOr<Retencion>> EjecutarAsync(ComandoEmitirRetencion cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.EmpresaRuc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.Retencion, empresa.Ruc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial);

        if (await retenciones.ExisteSecuencialActivoAsync(empresa.Ruc, cmd.Estab, cmd.PtoEmi, cmd.Secuencial, cmd.Ambiente, ct))
            return Errores.Retencion.SecuencialDuplicado;

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(cmd.EmpresaRuc, ct);

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

        var xmlResult = xml.GenerarXmlRetencion(retencion, empresa, parametros);
        if (xmlResult.IsError) return xmlResult.Errors;

        await retenciones.AgregarAsync(retencion, ct);

        return await orquestador.EjecutarAsync(new ParametrosEmision<Retencion>(
            retencion, claveAcceso, cmd.Ambiente, xmlResult.Value,
            $"{empresa.Ruc}/retenciones",
            empresa.CertificadoP12, empresa.CertPassword,
            (r, t) => pdf.GenerarRideRetencionAsync(r, t),
            (r, t) => retenciones.ActualizarAsync(r, t),
            async t =>
            {
                var sec = await secuenciales.ObtenerAsync(cmd.EmpresaRuc, "07", t);
                if (sec is not null) { sec.Incrementar(); await secuenciales.ActualizarAsync(sec, t); }
            }), ct);
    }
}
