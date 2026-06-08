using ErrorOr;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoActualizarEmpresa(
    string Ruc,
    string Nombre,
    string DirMatriz,
    Guid CuentaId,
    string? NombreComercial = null,
    byte[]? CertificadoP12 = null,
    string? CertPassword = null,
    byte[]? Logo = null,
    string? LogoContentType = null);

public class ActualizarEmpresa(IEmpresasRepositorio empresas, IServicioStorageFirmaYLogo storage, IUnitOfWork unitOfWork)
{
    public async Task<ErrorOr<Entidades.Empresa>> EjecutarAsync(ComandoActualizarEmpresa cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.Ruc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        if (empresa.CuentaId != cmd.CuentaId)
            return Errores.Empresa.Prohibido;

        empresa.ActualizarDatos(cmd.Nombre, cmd.DirMatriz, cmd.NombreComercial);

        if (cmd.CertificadoP12 is not null && !string.IsNullOrWhiteSpace(cmd.CertPassword))
        {
            var certPath = RutasStorage.Certificado(cmd.Ruc);
            var certResult = await storage.GuardarAsync(cmd.CertificadoP12, certPath, ct);
            if (certResult.IsError) return certResult.Errors;
            empresa.ActualizarCertificado(certResult.Value, cmd.CertPassword!);
        }

        if (cmd.Logo is not null)
        {
            var logoPath = RutasStorage.Logo(cmd.Ruc);
            var logoResult = await storage.GuardarAsync(cmd.Logo, logoPath, ct);
            if (logoResult.IsError) return logoResult.Errors;
            empresa.ActualizarLogo(logoResult.Value, cmd.LogoContentType);
        }

        await empresas.ActualizarAsync(empresa, ct);
        await unitOfWork.CommitAsync(ct);
        return empresa;
    }
}
