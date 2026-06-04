using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infraestructura.Persistencia.Repositorios;

public class CuentasRepositorio(AppDbContext context) : ICuentasRepositorio
{
    public async Task<Cuenta?> ObtenerPrimeraAsync(CancellationToken ct = default)
        => await context.Cuentas.FirstOrDefaultAsync(ct);

    public async Task<Cuenta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Cuentas.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<string>> ObtenerRucsPorCuentaAsync(Guid cuentaId, CancellationToken ct = default)
        => await context.Empresas
            .Where(e => e.CuentaId == cuentaId)
            .Select(e => e.Ruc)
            .ToListAsync(ct);

    public async Task<List<(string? XmlFirmado, string? XmlAutorizado, string? Pdf)>> ObtenerPathsDocumentosPorRucsAsync(
        IReadOnlyList<string> rucs, CancellationToken ct = default)
    {
        var facturaPaths = await context.Facturas
            .Where(f => rucs.Contains(f.EmpresaRuc))
            .Select(f => new { f.XmlFirmadoPath, f.XmlAutorizadoPath, f.PdfPath })
            .ToListAsync(ct);

        var ncPaths = await context.NotasCredito
            .Where(n => rucs.Contains(n.EmpresaRuc))
            .Select(n => new { n.XmlFirmadoPath, n.XmlAutorizadoPath, n.PdfPath })
            .ToListAsync(ct);

        var retPaths = await context.Retenciones
            .Where(r => rucs.Contains(r.EmpresaRuc))
            .Select(r => new { r.XmlFirmadoPath, r.XmlAutorizadoPath, r.PdfPath })
            .ToListAsync(ct);

        return facturaPaths.Select(x => (x.XmlFirmadoPath, x.XmlAutorizadoPath, x.PdfPath))
            .Concat(ncPaths.Select(x => (x.XmlFirmadoPath, x.XmlAutorizadoPath, x.PdfPath)))
            .Concat(retPaths.Select(x => (x.XmlFirmadoPath, x.XmlAutorizadoPath, x.PdfPath)))
            .ToList();
    }

    public async Task EliminarCuentaAsync(Guid cuentaId, IReadOnlyList<string> rucs, CancellationToken ct = default)
    {
        // Estados con obligación tributaria de 7 años — se anonimiza, no se borra
        var estadosFiscales = new[]
        {
            EstadoSri.Enviado, EstadoSri.PendienteAutorizacion,
            EstadoSri.AutorizadoPendienteArchivos, EstadoSri.Autorizado, EstadoSri.Anulado
        };

        if (rucs.Count > 0)
        {
            // ── Anonimizar documentos fiscales ────────────────────────────────
            await context.Facturas
                .Where(f => rucs.Contains(f.EmpresaRuc) && estadosFiscales.Contains(f.EstadoSri))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.RazonSocialComprador, "ANONIMIZADO")
                    .SetProperty(f => f.IdentificacionComprador, "ANONIMIZADO")
                    .SetProperty(f => f.DireccionComprador, (string?)null)
                    .SetProperty(f => f.IpAddress, (string?)null)
                    .SetProperty(f => f.XmlFirmadoPath, (string?)null)
                    .SetProperty(f => f.XmlAutorizadoPath, (string?)null)
                    .SetProperty(f => f.PdfPath, (string?)null), ct);

            await context.NotasCredito
                .Where(n => rucs.Contains(n.EmpresaRuc) && estadosFiscales.Contains(n.EstadoSri))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.RazonSocialComprador, "ANONIMIZADO")
                    .SetProperty(n => n.IdentificacionComprador, "ANONIMIZADO")
                    .SetProperty(n => n.DireccionComprador, (string?)null)
                    .SetProperty(n => n.IpAddress, (string?)null)
                    .SetProperty(n => n.XmlFirmadoPath, (string?)null)
                    .SetProperty(n => n.XmlAutorizadoPath, (string?)null)
                    .SetProperty(n => n.PdfPath, (string?)null), ct);

            await context.Retenciones
                .Where(r => rucs.Contains(r.EmpresaRuc) && estadosFiscales.Contains(r.EstadoSri))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.RazonSocialSujeto, "ANONIMIZADO")
                    .SetProperty(r => r.IdentificacionSujeto, "ANONIMIZADO")
                    .SetProperty(r => r.DireccionSujeto, (string?)null)
                    .SetProperty(r => r.IpAddress, (string?)null)
                    .SetProperty(r => r.XmlFirmadoPath, (string?)null)
                    .SetProperty(r => r.XmlAutorizadoPath, (string?)null)
                    .SetProperty(r => r.PdfPath, (string?)null), ct);

            // ── Hard delete documentos sin obligación fiscal (Pendiente, NoAutorizado)
            //    ON DELETE CASCADE en BD elimina _detalle automáticamente ───────
            await context.Facturas
                .Where(f => rucs.Contains(f.EmpresaRuc) && !estadosFiscales.Contains(f.EstadoSri))
                .ExecuteDeleteAsync(ct);

            await context.NotasCredito
                .Where(n => rucs.Contains(n.EmpresaRuc) && !estadosFiscales.Contains(n.EstadoSri))
                .ExecuteDeleteAsync(ct);

            await context.Retenciones
                .Where(r => rucs.Contains(r.EmpresaRuc) && !estadosFiscales.Contains(r.EstadoSri))
                .ExecuteDeleteAsync(ct);

            // ── Eliminar logs ─────────────────────────────────────────────────
            foreach (var ruc in rucs)
                await context.Database.ExecuteSqlAsync(
                    $"DELETE FROM facturacion.logs WHERE empresa_ruc = {ruc}", ct);

            // ── Eliminar empresas (cascade: parametros_facturacion, secuenciales_sri) ──
            await context.Empresas
                .Where(e => e.CuentaId == cuentaId)
                .ExecuteDeleteAsync(ct);
        }

        // ── Eliminar cuenta ───────────────────────────────────────────────────
        await context.Cuentas
            .Where(c => c.Id == cuentaId)
            .ExecuteDeleteAsync(ct);
    }
}
