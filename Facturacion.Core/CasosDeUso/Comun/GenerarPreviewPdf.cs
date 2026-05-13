using ErrorOr;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;

namespace Facturacion.Core.CasosDeUso.Comun;

public class GenerarPreviewPdf(
    IEmpresasRepositorio empresas,
    IParametrosFacturacionRepositorio parametrosRepo,
    IServicioPdf pdf,
    IServicioStorageFirmaYLogo storageFirma)
{
    private record Contexto(Empresa Empresa, ParametrosFacturacion? Parametros, byte[]? LogoBytes);

    public async Task<ErrorOr<byte[]>> EjecutarAsync(string empresaRuc, Factura factura, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(empresaRuc, ct);
        if (ctx.IsError) return ctx.Errors;
        return await pdf.GenerarRideFacturaAsync(factura, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    public async Task<ErrorOr<byte[]>> EjecutarAsync(string empresaRuc, NotaCredito nc, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(empresaRuc, ct);
        if (ctx.IsError) return ctx.Errors;
        return await pdf.GenerarRideNotaCreditoAsync(nc, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    public async Task<ErrorOr<byte[]>> EjecutarAsync(string empresaRuc, Retencion retencion, CancellationToken ct = default)
    {
        var ctx = await ObtenerContextoAsync(empresaRuc, ct);
        if (ctx.IsError) return ctx.Errors;
        return await pdf.GenerarRideRetencionAsync(retencion, ctx.Value.Empresa, ctx.Value.Parametros, ctx.Value.LogoBytes, ct);
    }

    private async Task<ErrorOr<Contexto>> ObtenerContextoAsync(string empresaRuc, CancellationToken ct)
    {
        var empresa = await empresas.ObtenerPorRucAsync(empresaRuc, ct);
        if (empresa is null) return Errores.Empresa.NoEncontrada;

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(empresaRuc, ct);

        byte[]? logoBytes = null;
        if (empresa.LogoPath is not null)
        {
            var logoResult = await storageFirma.ObtenerAsync(empresa.LogoPath, ct);
            if (!logoResult.IsError) logoBytes = logoResult.Value;
        }

        return new Contexto(empresa, parametros, logoBytes);
    }
}
