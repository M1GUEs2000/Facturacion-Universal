using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Repositorios;

namespace Facturacion.Core.CasosDeUso.Parametros;

public record ComandoGuardarSecuencialSri(
    string EmpresaRuc,
    string TipoComprobante,
    long Secuencial,
    string CodigoNumerico);

public class GuardarSecuencialSri(
    IEmpresasRepositorio empresas,
    ISecuencialesSriRepositorio secuencialesSri,
    IUnitOfWork unitOfWork)
{
    public async Task<ErrorOr<SecuencialSri>> EjecutarAsync(
        ComandoGuardarSecuencialSri cmd,
        CancellationToken ct = default)
    {
        if (!await empresas.ExisteAsync(cmd.EmpresaRuc, ct))
            return Error.NotFound("Empresa.NoEncontrada", "La empresa no existe.");

        var parametro = await secuencialesSri.ObtenerAsync(cmd.EmpresaRuc, cmd.TipoComprobante, ct);
        if (parametro is null)
        {
            parametro = SecuencialSri.Crear(cmd.EmpresaRuc, cmd.TipoComprobante, cmd.Secuencial, cmd.CodigoNumerico);
            await secuencialesSri.AgregarAsync(parametro, ct);
            await unitOfWork.CommitAsync(ct);
            return parametro;
        }

        parametro.Actualizar(cmd.Secuencial, cmd.CodigoNumerico);
        await secuencialesSri.ActualizarAsync(parametro, ct);
        await unitOfWork.CommitAsync(ct);
        return parametro;
    }
}
