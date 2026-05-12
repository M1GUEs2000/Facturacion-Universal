using Facturacion.Core.Entidades;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface IParametrosFacturacionRepositorio
{
    Task<ParametrosFacturacion?> ObtenerPorEmpresaAsync(string empresaRuc, CancellationToken ct = default);
    Task AgregarAsync(ParametrosFacturacion parametros, CancellationToken ct = default);
    Task ActualizarAsync(ParametrosFacturacion parametros, CancellationToken ct = default);
}
