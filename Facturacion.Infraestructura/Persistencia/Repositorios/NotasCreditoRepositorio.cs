using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infraestructura.Persistencia.Repositorios;

public class NotasCreditoRepositorio(AppDbContext context) : INotasCreditoRepositorio
{
    public async Task<NotaCredito?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.NotasCredito
            .Include(n => n.Detalle)
            .FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<NotaCredito?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default)
        => await context.NotasCredito
            .Include(n => n.Detalle)
            .FirstOrDefaultAsync(n => n.ClaveAcceso == claveAcceso, ct);

    public async Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default)
        => await context.NotasCredito.AnyAsync(n => n.ClaveAcceso == claveAcceso, ct);

    public async Task<IReadOnlyList<NotaCredito>> ListarPorEmpresaAsync(string empresaRuc, EstadoSri? estado = null, CancellationToken ct = default)
    {
        var query = context.NotasCredito.Where(n => n.EmpresaRuc == empresaRuc);
        if (estado.HasValue)
            query = query.Where(n => n.EstadoSri == estado.Value);
        return await query.OrderByDescending(n => n.FechaEmision).ToListAsync(ct);
    }

    public async Task AgregarAsync(NotaCredito notaCredito, CancellationToken ct = default)
    {
        await context.NotasCredito.AddAsync(notaCredito, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(NotaCredito notaCredito, CancellationToken ct = default)
    {
        context.NotasCredito.Update(notaCredito);
        await context.SaveChangesAsync(ct);
    }
}
