using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core;
using Facturacion.Core.Interfaces.Repositorios;

namespace Facturacion.Core.CasosDeUso.Empresas;

public record ComandoRegistrarEmpresa(
    string Ruc,
    string Nombre,
    string DirMatriz,
    bool ObligadoContabilidad,
    byte[] CertificadoP12,
    string CertPassword,
    string? NombreComercial = null);

public class RegistrarEmpresa(IEmpresasRepositorio empresas)
{
    public async Task<ErrorOr<Empresa>> EjecutarAsync(ComandoRegistrarEmpresa cmd, CancellationToken ct = default)
    {
        if (await empresas.ExisteAsync(cmd.Ruc, ct))
            return Errores.Empresa.RucDuplicado;

        var empresa = Empresa.Crear(
            cmd.Ruc, cmd.Nombre, cmd.DirMatriz, cmd.ObligadoContabilidad,
            cmd.CertificadoP12, cmd.CertPassword, cmd.NombreComercial);
        await empresas.AgregarAsync(empresa, ct);

        return empresa;
    }
}
