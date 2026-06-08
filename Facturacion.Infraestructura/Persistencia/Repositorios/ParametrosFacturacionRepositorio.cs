using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infraestructura.Persistencia.Repositorios;

public class ParametrosFacturacionRepositorio(AppDbContext context) : IParametrosFacturacionRepositorio
{
    public async Task<ParametrosFacturacion?> ObtenerPorEmpresaAsync(string empresaRuc, CancellationToken ct = default)
        => await context.ParametrosFacturacion.FirstOrDefaultAsync(p => p.EmpresaRuc == empresaRuc, ct);

    public async Task AgregarAsync(ParametrosFacturacion parametros, CancellationToken ct = default)
        => await context.ParametrosFacturacion.AddAsync(parametros, ct);

    public Task ActualizarAsync(ParametrosFacturacion parametros, CancellationToken ct = default)
    {
        context.ParametrosFacturacion.Update(parametros);
        return Task.CompletedTask;
    }
}
