using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;

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

public class RegistrarEmpresa(IEmpresasRepositorio empresas, ICuentasRepositorio cuentas)
{
    public async Task<ErrorOr<Empresa>> EjecutarAsync(ComandoRegistrarEmpresa cmd, CancellationToken ct = default)
    {
        if (await empresas.ExisteAsync(cmd.Ruc, ct))
            return Errores.Empresa.RucDuplicado;

        var cuenta = await cuentas.ObtenerPrimeraAsync(ct);
        if (cuenta is null)
            return Error.NotFound("Cuenta.NoEncontrada", "No existe ninguna cuenta configurada.");

        var empresa = Empresa.Crear(
            cmd.Ruc, cmd.Nombre, cmd.DirMatriz,
            cmd.CertificadoP12, cmd.CertPassword, cuenta.Id,
            cmd.NombreComercial, cmd.Logo, cmd.LogoContentType);

        await empresas.AgregarAsync(empresa, ct);
        return empresa;
    }
}
