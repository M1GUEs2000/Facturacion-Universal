using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoRegistrarEmpresa(
    string Ruc,
    string Nombre,
    string DirMatriz,
    byte[] CertificadoP12,
    string CertPassword,
    string? NombreComercial = null,
    byte[]? Logo = null,
    string? LogoContentType = null);

public class RegistrarEmpresa(
    IEmpresasRepositorio empresas,
    ICuentasRepositorio cuentas,
    IServicioStorageFirmaYLogo storage)
{
    public async Task<ErrorOr<Empresa>> EjecutarAsync(ComandoRegistrarEmpresa cmd, CancellationToken ct = default)
    {
        if (await empresas.ExisteAsync(cmd.Ruc, ct))
            return Errores.Empresa.RucDuplicado;

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

        var empresa = Empresa.Crear(
            cmd.Ruc, cmd.Nombre, cmd.DirMatriz,
            certResult.Value, cmd.CertPassword, cuenta.Id,
            cmd.NombreComercial, logoPath, cmd.LogoContentType);

        await empresas.AgregarAsync(empresa, ct);
        return empresa;
    }
}
