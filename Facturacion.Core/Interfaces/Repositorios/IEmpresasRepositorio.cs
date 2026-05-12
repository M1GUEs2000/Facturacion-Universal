using Facturacion.Core.Entidades;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface IEmpresasRepositorio
{
    Task<List<Empresa>> ListarAsync(CancellationToken ct = default);
    Task<Empresa?> ObtenerPorRucAsync(string ruc, CancellationToken ct = default);
    Task<bool> ExisteAsync(string ruc, CancellationToken ct = default);
    Task AgregarAsync(Empresa empresa, CancellationToken ct = default);
    Task ActualizarAsync(Empresa empresa, CancellationToken ct = default);
}
