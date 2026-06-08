using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Repositorios;

namespace Facturacion.Core.CasosDeUso.Parametros;

public record ComandoGuardarParametrosFacturacion(
    string EmpresaRuc,
    Ambiente Ambiente,
    string TipoEmision,
    bool AgenteRetencion,
    string? ContribuyenteRimpe,
    string Estab,
    string PuntoEmision,
    string? ContribuyenteEspecial,
    bool ObligadoContabilidad,
    string Moneda,
    string CodigoImpuesto,
    CodigoIva CodigoPorcentaje);

public class GuardarParametrosFacturacion(
    IEmpresasRepositorio empresas,
    IParametrosFacturacionRepositorio parametrosRepositorio,
    IUnitOfWork unitOfWork)
{
    public async Task<ErrorOr<ParametrosFacturacion>> EjecutarAsync(
        ComandoGuardarParametrosFacturacion cmd,
        CancellationToken ct = default)
    {
        if (!await empresas.ExisteAsync(cmd.EmpresaRuc, ct))
            return Error.NotFound("Empresa.NoEncontrada", "La empresa no existe.");

        var parametros = await parametrosRepositorio.ObtenerPorEmpresaAsync(cmd.EmpresaRuc, ct);
        if (parametros is null)
        {
            parametros = ParametrosFacturacion.Crear(
                cmd.EmpresaRuc, cmd.Ambiente, cmd.TipoEmision, cmd.AgenteRetencion,
                cmd.ContribuyenteRimpe, cmd.Estab, cmd.PuntoEmision,
                cmd.ContribuyenteEspecial, cmd.ObligadoContabilidad, cmd.Moneda,
                cmd.CodigoImpuesto, cmd.CodigoPorcentaje);

            await parametrosRepositorio.AgregarAsync(parametros, ct);
            await unitOfWork.CommitAsync(ct);
            return parametros;
        }

        parametros.Actualizar(
            cmd.Ambiente, cmd.TipoEmision, cmd.AgenteRetencion, cmd.ContribuyenteRimpe,
            cmd.Estab, cmd.PuntoEmision, cmd.ContribuyenteEspecial,
            cmd.ObligadoContabilidad, cmd.Moneda, cmd.CodigoImpuesto, cmd.CodigoPorcentaje);

        await parametrosRepositorio.ActualizarAsync(parametros, ct);
        await unitOfWork.CommitAsync(ct);
        return parametros;
    }
}
