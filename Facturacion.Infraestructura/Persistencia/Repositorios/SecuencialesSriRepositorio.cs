using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Facturacion.Infraestructura.Persistencia.Repositorios;

public class SecuencialesSriRepositorio(AppDbContext context) : ISecuencialesSriRepositorio
{
    public async Task<IReadOnlyList<SecuencialSri>> ListarPorEmpresaAsync(string empresaRuc, CancellationToken ct = default)
        => await context.SecuencialesSri
            .Where(p => p.EmpresaRuc == empresaRuc)
            .OrderBy(p => p.TipoComprobante)
            .ToListAsync(ct);

    public async Task<SecuencialSri?> ObtenerAsync(string empresaRuc, string tipoComprobante, CancellationToken ct = default)
        => await context.SecuencialesSri
            .FirstOrDefaultAsync(p => p.EmpresaRuc == empresaRuc && p.TipoComprobante == tipoComprobante, ct);

    public async Task AgregarAsync(SecuencialSri parametro, CancellationToken ct = default)
    {
        await context.SecuencialesSri.AddAsync(parametro, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(SecuencialSri parametro, CancellationToken ct = default)
    {
        context.SecuencialesSri.Update(parametro);
        await context.SaveChangesAsync(ct);
    }

    public async Task<long> IncrementarYObtenerAsync(string empresaRuc, string tipoComprobante, CancellationToken ct = default)
    {
        // UPDATE atómico — PostgreSQL garantiza que dos requests concurrentes
        // obtienen números distintos sin necesidad de locks en la aplicación.
        var result = await context.Database.SqlQueryRaw<long>(
            """
            UPDATE secuenciales_sri
            SET secuencial           = secuencial + 1,
                fecha_actualizacion  = NOW(),
                updated_at           = NOW()
            WHERE empresa_ruc      = {0}
              AND tipo_comprobante = {1}
            RETURNING secuencial
            """,
            empresaRuc, tipoComprobante)
            .ToListAsync(ct);

        if (result.Count == 0)
            throw new InvalidOperationException(
                $"No existe secuencial para RUC {empresaRuc} / tipo {tipoComprobante}.");

        return result[0];
    }
}
