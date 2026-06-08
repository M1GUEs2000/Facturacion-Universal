using ErrorOr;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

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
    public async Task<ErrorOr<Retencion>> EjecutarAsync(Guid retencionId, Guid cuentaId, CancellationToken ct = default)
    {
        var retencion = await retenciones.ObtenerPorIdAsync(retencionId, ct);
        if (retencion is null) return Errores.Retencion.NoEncontrada;

        var empresa = await empresas.ObtenerPorRucAsync(retencion.EmpresaRuc, ct);
        if (empresa is null) return Errores.Empresa.NoEncontrada;
        if (empresa.CuentaId != cuentaId) return Errores.Empresa.Prohibido;

        if (retencion.EstadoSri is Enums.EstadoSri.Autorizado or Enums.EstadoSri.NoAutorizado or Enums.EstadoSri.Anulado)
            return Errores.Retencion.EstadoInvalido;

        var certResult = await storageFirma.ObtenerAsync(empresa.CertificadoPath, ct);
        if (certResult.IsError) return certResult.Errors;

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(retencion.EmpresaRuc, ct);

        byte[]? logoBytes = null;
        if (empresa.LogoPath is not null)
        {
            var logoResult = await storageFirma.ObtenerAsync(empresa.LogoPath, ct);
            if (!logoResult.IsError) logoBytes = logoResult.Value;
        }

        return await orquestador.EjecutarAsync(new ParametrosReintento<Retencion>(
            retencion,
            retencion.ClaveAcceso,
            retencion.Ambiente,
            RutasStorage.PrefijoRetenciones(empresa.Ruc),
            certResult.Value,
            empresa.CertPassword,
            (r, _) => xml.GenerarXmlRetencion(r, empresa, parametros),
            (r, t) => pdf.GenerarRideRetencionAsync(r, empresa, parametros, logoBytes, t)), ct);
    }
}
