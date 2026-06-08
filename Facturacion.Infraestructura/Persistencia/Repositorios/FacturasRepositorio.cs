using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Comun;
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

    public async Task<bool> ExisteSecuencialActivoAsync(
        string empresaRuc, string estab, string ptoEmi, string secuencial, Ambiente ambiente, CancellationToken ct = default)
        => await context.Facturas.AnyAsync(f =>
            f.EmpresaRuc == empresaRuc &&
            f.Estab == estab &&
            f.PtoEmi == ptoEmi &&
            f.Secuencial == secuencial &&
            f.Ambiente == ambiente &&
            f.EstadoSri != EstadoSri.Pendiente &&
            f.EstadoSri != EstadoSri.NoAutorizado, ct);

    public async Task<IReadOnlyList<Factura>> ListarConCursorAsync(
        string empresaRuc, EstadoSri? estado, CursorDePagina? cursor, int tamanoPagina, CancellationToken ct = default)
    {
        var query = context.Facturas.Where(f => f.EmpresaRuc == empresaRuc);
        if (estado.HasValue)
            query = query.Where(f => f.EstadoSri == estado.Value);
        if (cursor is not null)
            query = query.Where(f => f.CreatedAt < cursor.CreatedAt);
        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ThenByDescending(f => f.Id)
            .Take(tamanoPagina + 1)
            .ToListAsync(ct);
    }

    public async Task AgregarAsync(Factura factura, CancellationToken ct = default)
        => await context.Facturas.AddAsync(factura, ct);

    public Task ActualizarAsync(Factura factura, CancellationToken ct = default)
    {
        context.Facturas.Update(factura);
        return Task.CompletedTask;
    }
}
