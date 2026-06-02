using Facturacion.Core.Entidades;

namespace Facturacion.Core.Interfaces.Repositorios;

public interface ISecuencialesSriRepositorio
{
    Task<IReadOnlyList<SecuencialSri>> ListarPorEmpresaAsync(string empresaRuc, CancellationToken ct = default);
    Task<SecuencialSri?> ObtenerAsync(string empresaRuc, string tipoComprobante, CancellationToken ct = default);
    Task AgregarAsync(SecuencialSri parametro, CancellationToken ct = default);
    Task ActualizarAsync(SecuencialSri parametro, CancellationToken ct = default);
    Task<long> IncrementarYObtenerAsync(string empresaRuc, string tipoComprobante, CancellationToken ct = default);
}
