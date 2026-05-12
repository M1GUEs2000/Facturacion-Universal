using ErrorOr;
using Facturacion.Core.Interfaces.Repositorios;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoGuardarEmpresa(
    string Ruc,
    string Nombre,
    string DirMatriz,
    string? NombreComercial = null,
    byte[]? CertificadoP12 = null,
    string? CertPassword = null,
    byte[]? Logo = null,
    string? LogoContentType = null);

public class GuardarEmpresa(IEmpresasRepositorio empresas, ICuentasRepositorio cuentas)
{
    public async Task<ErrorOr<Entidades.Empresa>> EjecutarAsync(ComandoGuardarEmpresa cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.Ruc, ct);

        if (empresa is null)
        {
            if (cmd.CertificadoP12 is null || string.IsNullOrWhiteSpace(cmd.CertPassword))
                return Error.Validation("Empresa.CertificadoRequerido", "La firma .p12 y su clave son requeridas para registrar la empresa.");

            var cuenta = await cuentas.ObtenerPrimeraAsync(ct);
            if (cuenta is null)
                return Error.NotFound("Cuenta.NoEncontrada", "No existe ninguna cuenta configurada.");

            empresa = Entidades.Empresa.Crear(
                cmd.Ruc, cmd.Nombre, cmd.DirMatriz,
                cmd.CertificadoP12, cmd.CertPassword, cuenta.Id,
                cmd.NombreComercial, cmd.Logo, cmd.LogoContentType);

            await empresas.AgregarAsync(empresa, ct);
            return empresa;
        }

        empresa.ActualizarDatos(cmd.Nombre, cmd.DirMatriz, cmd.NombreComercial);

        if (cmd.CertificadoP12 is not null && !string.IsNullOrWhiteSpace(cmd.CertPassword))
            empresa.ActualizarCertificado(cmd.CertificadoP12, cmd.CertPassword);

        if (cmd.Logo is not null)
            empresa.ActualizarLogo(cmd.Logo, cmd.LogoContentType);

        await empresas.ActualizarAsync(empresa, ct);
        return empresa;
    }
}
