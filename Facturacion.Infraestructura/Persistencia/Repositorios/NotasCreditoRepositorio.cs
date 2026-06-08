using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Comun;
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

    public async Task<bool> ExisteSecuencialActivoAsync(
        string empresaRuc, string estab, string ptoEmi, string secuencial, Ambiente ambiente, CancellationToken ct = default)
        => await context.NotasCredito.AnyAsync(n =>
            n.EmpresaRuc == empresaRuc &&
            n.Estab == estab &&
            n.PtoEmi == ptoEmi &&
            n.Secuencial == secuencial &&
            n.Ambiente == ambiente &&
            n.EstadoSri != EstadoSri.Pendiente &&
            n.EstadoSri != EstadoSri.NoAutorizado, ct);

    public async Task<IReadOnlyList<NotaCredito>> ListarConCursorAsync(
        string empresaRuc, EstadoSri? estado, CursorDePagina? cursor, int tamanoPagina, CancellationToken ct = default)
    {
        var query = context.NotasCredito.Where(n => n.EmpresaRuc == empresaRuc);
        if (estado.HasValue)
            query = query.Where(n => n.EstadoSri == estado.Value);
        if (cursor is not null)
            query = query.Where(n => n.CreatedAt < cursor.CreatedAt);
        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(tamanoPagina + 1)
            .ToListAsync(ct);
    }

    public async Task AgregarAsync(NotaCredito notaCredito, CancellationToken ct = default)
        => await context.NotasCredito.AddAsync(notaCredito, ct);

    public Task ActualizarAsync(NotaCredito notaCredito, CancellationToken ct = default)
    {
        context.NotasCredito.Update(notaCredito);
        return Task.CompletedTask;
    }
}
