using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

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
}
