namespace Facturacion.Api.Contratos.Comun;

public record PaginaResponse<T>(
    IReadOnlyList<T> Data,
    int Total,
    int Pagina,
    int TamanoPagina,
    bool HasNextPage);
