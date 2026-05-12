using ErrorOr;
using Facturacion.Core.Interfaces.Repositorios;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoActualizarEmpresa(
    string Ruc,
    string Nombre,
    string DirMatriz,
    string? NombreComercial = null,
    byte[]? CertificadoP12 = null,
    string? CertPassword = null,
    byte[]? Logo = null,
    string? LogoContentType = null);

public class ActualizarEmpresa(IEmpresasRepositorio empresas)
{
    public async Task<ErrorOr<Entidades.Empresa>> EjecutarAsync(ComandoActualizarEmpresa cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.Ruc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        empresa.ActualizarDatos(cmd.Nombre, cmd.DirMatriz, cmd.NombreComercial);

        if (cmd.CertificadoP12 is not null && !string.IsNullOrWhiteSpace(cmd.CertPassword))
            empresa.ActualizarCertificado(cmd.CertificadoP12, cmd.CertPassword);

        if (cmd.Logo is not null)
            empresa.ActualizarLogo(cmd.Logo, cmd.LogoContentType);

        await empresas.ActualizarAsync(empresa, ct);
        return empresa;
    }
}
