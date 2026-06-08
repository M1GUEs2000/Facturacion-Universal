using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Comun;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infraestructura.Persistencia.Repositorios;

public class RetencionesRepositorio(AppDbContext context) : IRetencionesRepositorio
{
    public async Task<Retencion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await context.Retenciones
            .Include(r => r.Detalle)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Retencion?> ObtenerPorClaveAccesoAsync(string claveAcceso, CancellationToken ct = default)
        => await context.Retenciones
            .Include(r => r.Detalle)
            .FirstOrDefaultAsync(r => r.ClaveAcceso == claveAcceso, ct);

    public async Task<bool> ExisteClaveAccesoAsync(string claveAcceso, CancellationToken ct = default)
        => await context.Retenciones.AnyAsync(r => r.ClaveAcceso == claveAcceso, ct);

    public async Task<bool> ExisteSecuencialActivoAsync(
        string empresaRuc, string estab, string ptoEmi, string secuencial, Ambiente ambiente, CancellationToken ct = default)
        => await context.Retenciones.AnyAsync(r =>
            r.EmpresaRuc == empresaRuc &&
            r.Estab == estab &&
            r.PtoEmi == ptoEmi &&
            r.Secuencial == secuencial &&
            r.Ambiente == ambiente &&
            r.EstadoSri != EstadoSri.Pendiente &&
            r.EstadoSri != EstadoSri.NoAutorizado, ct);

    public async Task<IReadOnlyList<Retencion>> ListarConCursorAsync(
        string empresaRuc, EstadoSri? estado, CursorDePagina? cursor, int tamanoPagina, CancellationToken ct = default)
    {
        var query = context.Retenciones.Where(r => r.EmpresaRuc == empresaRuc);
        if (estado.HasValue)
            query = query.Where(r => r.EstadoSri == estado.Value);
        if (cursor is not null)
            query = query.Where(r => r.CreatedAt < cursor.CreatedAt);
        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Take(tamanoPagina + 1)
            .ToListAsync(ct);
    }

    public async Task AgregarAsync(Retencion retencion, CancellationToken ct = default)
        => await context.Retenciones.AddAsync(retencion, ct);

    public Task ActualizarAsync(Retencion retencion, CancellationToken ct = default)
    {
        context.Retenciones.Update(retencion);
        return Task.CompletedTask;
    }
}
