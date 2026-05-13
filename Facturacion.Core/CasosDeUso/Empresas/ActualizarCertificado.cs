using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoActualizarCertificado(
    string Ruc,
    byte[] CertificadoP12,
    string CertPassword);

public class ActualizarCertificado(IEmpresasRepositorio empresas, IServicioStorageFirmaYLogo storage)
{
    public async Task<ErrorOr<Empresa>> EjecutarAsync(ComandoActualizarCertificado cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.Ruc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        var certPath = $"{cmd.Ruc}/certificado.p12";
        var certResult = await storage.GuardarAsync(cmd.CertificadoP12, certPath, ct);
        if (certResult.IsError) return certResult.Errors;

        empresa.ActualizarCertificado(certResult.Value, cmd.CertPassword);
        await empresas.ActualizarAsync(empresa, ct);

        return empresa;
    }
}
