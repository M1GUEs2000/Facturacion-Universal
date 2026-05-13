using ErrorOr;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.NotasCredito;

public class ReintentarEmisionNotaCredito(
    IEmpresasRepositorio empresas,
    INotasCreditoRepositorio notasCredito,
    IParametrosFacturacionRepositorio parametrosRepo,
    IServicioXml xml,
    IServicioPdf pdf,
    IServicioStorageFirmaYLogo storageFirma,
    OrquestadorReintento orquestador)
{
    public async Task<ErrorOr<NotaCredito>> EjecutarAsync(Guid notaId, CancellationToken ct = default)
    {
        var nota = await notasCredito.ObtenerPorIdAsync(notaId, ct);
        if (nota is null) return Errores.NotaCredito.NoEncontrada;

        if (nota.EstadoSri is Enums.EstadoSri.Autorizado or Enums.EstadoSri.NoAutorizado or Enums.EstadoSri.Anulado)
            return Errores.NotaCredito.EstadoInvalido;

        var empresa = await empresas.ObtenerPorRucAsync(nota.EmpresaRuc, ct);
        if (empresa is null) return Errores.Empresa.NoEncontrada;

        var certResult = await storageFirma.ObtenerAsync(empresa.CertificadoPath, ct);
        if (certResult.IsError) return certResult.Errors;

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(nota.EmpresaRuc, ct);

        byte[]? logoBytes = null;
        if (empresa.LogoPath is not null)
        {
            var logoResult = await storageFirma.ObtenerAsync(empresa.LogoPath, ct);
            if (!logoResult.IsError) logoBytes = logoResult.Value;
        }

        return await orquestador.EjecutarAsync(new ParametrosReintento<NotaCredito>(
            nota,
            nota.ClaveAcceso,
            nota.Ambiente,
            RutasStorage.PrefijoNotasCredito(empresa.Ruc),
            certResult.Value,
            empresa.CertPassword,
            (n, _) => xml.GenerarXmlNotaCredito(n, empresa),
            (n, t) => pdf.GenerarRideNotaCreditoAsync(n, empresa, parametros, logoBytes, t),
            (n, t) => notasCredito.ActualizarAsync(n, t)), ct);
    }
}
