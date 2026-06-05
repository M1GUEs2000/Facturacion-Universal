using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infraestructura.Persistencia.Repositorios;

public class IdempotencyRepositorio(AppDbContext context) : IIdempotencyRepositorio
{
    public async Task<IdempotencyRecord?> ObtenerAsync(string key, Guid cuentaId, CancellationToken ct = default)
        => await context.IdempotencyKeys
            .FirstOrDefaultAsync(
                x => x.Key == key && x.CuentaId == cuentaId && x.ExpiresAt > DateTime.UtcNow, ct);

    public async Task GuardarAsync(IdempotencyRecord record, CancellationToken ct = default)
    {
        try
        {
            await context.IdempotencyKeys.AddAsync(record, ct);
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // concurrent request with same key already stored — ignore
        }
    }
}
