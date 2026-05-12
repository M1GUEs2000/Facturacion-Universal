using Facturacion.Core.Entidades;
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
}
