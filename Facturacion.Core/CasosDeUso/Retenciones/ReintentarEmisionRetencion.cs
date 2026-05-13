using ErrorOr;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;

namespace Facturacion.Core.CasosDeUso.Retenciones;

public class ReintentarEmisionRetencion(
    IEmpresasRepositorio empresas,
    IRetencionesRepositorio retenciones,
    IParametrosFacturacionRepositorio parametrosRepo,
    IServicioXml xml,
    IServicioPdf pdf,
    IServicioStorageFirmaYLogo storageFirma,
    OrquestadorReintento orquestador)
{
    public async Task<ErrorOr<Retencion>> EjecutarAsync(Guid retencionId, CancellationToken ct = default)
    {
        var retencion = await retenciones.ObtenerPorIdAsync(retencionId, ct);
        if (retencion is null) return Errores.Retencion.NoEncontrada;

        if (retencion.EstadoSri is Enums.EstadoSri.Autorizado or Enums.EstadoSri.NoAutorizado or Enums.EstadoSri.Anulado)
            return Errores.Retencion.EstadoInvalido;

        var empresa = await empresas.ObtenerPorRucAsync(retencion.EmpresaRuc, ct);
        if (empresa is null) return Errores.Empresa.NoEncontrada;

        var certResult = await storageFirma.ObtenerAsync(empresa.CertificadoPath, ct);
        if (certResult.IsError) return certResult.Errors;

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(retencion.EmpresaRuc, ct);

        return await orquestador.EjecutarAsync(new ParametrosReintento<Retencion>(
            retencion,
            retencion.ClaveAcceso,
            retencion.Ambiente,
            $"{empresa.Ruc}/retenciones",
            certResult.Value,
            empresa.CertPassword,
            (r, _) => xml.GenerarXmlRetencion(r, empresa, parametros),
            (r, t) => pdf.GenerarRideRetencionAsync(r, t),
            (r, t) => retenciones.ActualizarAsync(r, t)), ct);
    }
}
