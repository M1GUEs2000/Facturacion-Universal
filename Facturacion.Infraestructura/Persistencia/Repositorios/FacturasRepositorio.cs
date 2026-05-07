using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infraestructura.Persistencia.Repositorios;

public class FacturasRepositorio(AppDbContext context) : IFacturasRepositorio
{
    public async Task<Factura?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Facturas
            .Include(f => f.Detalle)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<Factura?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default)
        => await context.Facturas
            .Include(f => f.Detalle)
            .FirstOrDefaultAsync(f => f.ClaveAcceso == claveAcceso, ct);

    public async Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default)
        => await context.Facturas.AnyAsync(f => f.ClaveAcceso == claveAcceso, ct);

    public async Task<IReadOnlyList<Factura>> ListarPorEmpresaAsync(string empresaRuc, EstadoSri? estado = null, CancellationToken ct = default)
    {
        var query = context.Facturas.Where(f => f.EmpresaRuc == empresaRuc);
        if (estado.HasValue)
            query = query.Where(f => f.EstadoSri == estado.Value);
        return await query.OrderByDescending(f => f.FechaEmision).ToListAsync(ct);
    }

    public async Task AgregarAsync(Factura factura, CancellationToken ct = default)
    {
        await context.Facturas.AddAsync(factura, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Factura factura, CancellationToken ct = default)
    {
        context.Facturas.Update(factura);
        await context.SaveChangesAsync(ct);
    }
}
