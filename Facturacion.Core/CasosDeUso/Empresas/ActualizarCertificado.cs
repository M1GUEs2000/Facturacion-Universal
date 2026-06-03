using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoActualizarCertificado(
    string Ruc,
    byte[] CertificadoP12,
    string CertPassword,
    Guid CuentaId);

public class ActualizarCertificado(IEmpresasRepositorio empresas, IServicioStorageFirmaYLogo storage)
{
    public async Task<ErrorOr<Empresa>> EjecutarAsync(ComandoActualizarCertificado cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.Ruc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        if (empresa.CuentaId != cmd.CuentaId)
            return Errores.Empresa.Prohibido;

        var certPath = RutasStorage.Certificado(cmd.Ruc);
        var certResult = await storage.GuardarAsync(cmd.CertificadoP12, certPath, ct);
        if (certResult.IsError) return certResult.Errors;

        empresa.ActualizarCertificado(certResult.Value, cmd.CertPassword);
        await empresas.ActualizarAsync(empresa, ct);

        return empresa;
    }
}
