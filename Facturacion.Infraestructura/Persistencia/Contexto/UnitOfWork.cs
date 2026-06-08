using Facturacion.Core.Interfaces;

namespace Facturacion.Infraestructura.Persistencia.Contexto;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task CommitAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
