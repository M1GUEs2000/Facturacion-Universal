using ErrorOr;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;

namespace Facturacion.Core.CasosDeUso.NotasCredito;

public class ReintentarEmisionNotaCredito(
    IEmpresasRepositorio empresas,
    INotasCreditoRepositorio notasCredito,
    IServicioXml xml,
    IServicioPdf pdf,
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

        return await orquestador.EjecutarAsync(new ParametrosReintento<NotaCredito>(
            nota,
            nota.ClaveAcceso,
            nota.Ambiente,
            $"{empresa.Ruc}/notas-credito",
            empresa.CertificadoP12,
            empresa.CertPassword,
            (n, _) => xml.GenerarXmlNotaCredito(n, empresa),
            (n, t) => pdf.GenerarRideNotaCreditoAsync(n, t),
            (n, t) => notasCredito.ActualizarAsync(n, t)), ct);
    }
}
