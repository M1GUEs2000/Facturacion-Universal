using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoActualizarCertificado(
    string Ruc,
    byte[] CertificadoP12,
    string CertPassword);

public class ActualizarCertificado(IEmpresasRepositorio empresas)
{
    public async Task<ErrorOr<Empresa>> EjecutarAsync(ComandoActualizarCertificado cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.Ruc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        empresa.ActualizarCertificado(cmd.CertificadoP12, cmd.CertPassword);
        await empresas.ActualizarAsync(empresa, ct);

        return empresa;
    }
}
