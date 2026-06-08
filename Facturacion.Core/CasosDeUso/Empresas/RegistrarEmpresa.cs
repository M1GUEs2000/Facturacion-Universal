using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoRegistrarEmpresa(
    string Ruc,
    string Nombre,
    string DirMatriz,
    byte[] CertificadoP12,
    string CertPassword,
    Guid CuentaId,
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
        var cuenta = await cuentas.ObtenerPorIdAsync(cmd.CuentaId, ct);
        if (cuenta is null) return Errores.Cuenta.NoEncontrada;

        if (cuenta.FechaExpira.HasValue && cuenta.FechaExpira.Value < DateTimeOffset.UtcNow)
            return Errores.Cuenta.Expirada;

        var totalEmpresas = await empresas.ContarPorCuentaAsync(cmd.CuentaId, ct);
        if (totalEmpresas >= cuenta.MaxEmpresas) return Errores.Cuenta.LimiteEmpresas;

        if (await empresas.ExisteAsync(cmd.Ruc, ct))
            return Errores.Empresa.RucDuplicado;

        var certPath = RutasStorage.Certificado(cmd.Ruc);
        var certResult = await storage.GuardarAsync(cmd.CertificadoP12, certPath, ct);
        if (certResult.IsError) return certResult.Errors;

        string? logoPath = null;
        if (cmd.Logo is not null)
        {
            logoPath = RutasStorage.Logo(cmd.Ruc);
            var logoResult = await storage.GuardarAsync(cmd.Logo, logoPath, ct);
            if (logoResult.IsError) return logoResult.Errors;
        }

        var empresa = Empresa.Crear(
            cmd.Ruc, cmd.Nombre, cmd.DirMatriz,
            certResult.Value, cmd.CertPassword, cmd.CuentaId,
            cmd.NombreComercial, logoPath, cmd.LogoContentType);

        await empresas.AgregarAsync(empresa, ct);
        return empresa;
    }
}
