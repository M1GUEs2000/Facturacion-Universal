using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infraestructura.Persistencia.Repositorios;

public class EmpresasRepositorio(AppDbContext context) : IEmpresasRepositorio
{
    public async Task<Empresa?> ObtenerPorRucAsync(string ruc, CancellationToken ct = default)
        => await context.Empresas.FirstOrDefaultAsync(e => e.Ruc == ruc, ct);

    public async Task<bool> ExisteAsync(string ruc, CancellationToken ct = default)
        => await context.Empresas.AnyAsync(e => e.Ruc == ruc, ct);

    public async Task AgregarAsync(Empresa empresa, CancellationToken ct = default)
    {
        await context.Empresas.AddAsync(empresa, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Empresa empresa, CancellationToken ct = default)
    {
        context.Empresas.Update(empresa);
        await context.SaveChangesAsync(ct);
    }
}
