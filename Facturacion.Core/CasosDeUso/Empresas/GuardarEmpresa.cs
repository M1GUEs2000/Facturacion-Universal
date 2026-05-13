using ErrorOr;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;

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

public class GuardarEmpresa(
    IEmpresasRepositorio empresas,
    ICuentasRepositorio cuentas,
    IServicioStorageFirmaYLogo storage)
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

            var certPath = $"{cmd.Ruc}/certificado.p12";
            var certResult = await storage.GuardarAsync(cmd.CertificadoP12, certPath, ct);
            if (certResult.IsError) return certResult.Errors;

            string? logoPath = null;
            if (cmd.Logo is not null)
            {
                logoPath = $"{cmd.Ruc}/logo";
                var logoResult = await storage.GuardarAsync(cmd.Logo, logoPath, ct);
                if (logoResult.IsError) return logoResult.Errors;
            }

            empresa = Entidades.Empresa.Crear(
                cmd.Ruc, cmd.Nombre, cmd.DirMatriz,
                certResult.Value, cmd.CertPassword!, cuenta.Id,
                cmd.NombreComercial, logoPath, cmd.LogoContentType);

            await empresas.AgregarAsync(empresa, ct);
            return empresa;
        }

        empresa.ActualizarDatos(cmd.Nombre, cmd.DirMatriz, cmd.NombreComercial);

        if (cmd.CertificadoP12 is not null && !string.IsNullOrWhiteSpace(cmd.CertPassword))
        {
            var certPath = $"{cmd.Ruc}/certificado.p12";
            var certResult = await storage.GuardarAsync(cmd.CertificadoP12, certPath, ct);
            if (certResult.IsError) return certResult.Errors;
            empresa.ActualizarCertificado(certResult.Value, cmd.CertPassword!);
        }

        if (cmd.Logo is not null)
        {
            var logoPath = $"{cmd.Ruc}/logo";
            var logoResult = await storage.GuardarAsync(cmd.Logo, logoPath, ct);
            if (logoResult.IsError) return logoResult.Errors;
            empresa.ActualizarLogo(logoResult.Value, cmd.LogoContentType);
        }

        await empresas.ActualizarAsync(empresa, ct);
        return empresa;
    }
}
