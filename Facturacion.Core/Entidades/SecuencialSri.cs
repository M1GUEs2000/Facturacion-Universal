namespace Facturacion.Core.Entidades;

public class SecuencialSri
{
    protected SecuencialSri() { }

    public Guid Id { get; private set; }
    public string EmpresaRuc { get; private set; } = null!;
    public string TipoComprobante { get; private set; } = null!;
    public long Secuencial { get; private set; }
    public string CodigoNumerico { get; private set; } = null!;
    public DateTimeOffset FechaActualizacion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static SecuencialSri Crear(
        string empresaRuc,
        string tipoComprobante,
        long secuencial,
        string codigoNumerico)
    {
        var ahora = DateTimeOffset.UtcNow;
        return new SecuencialSri
        {
            Id = Guid.NewGuid(),
            EmpresaRuc = empresaRuc,
            TipoComprobante = tipoComprobante,
            Secuencial = secuencial <= 0 ? 1 : secuencial,
            CodigoNumerico = NormalizarCodigoNumerico(codigoNumerico),
            FechaActualizacion = ahora,
            CreatedAt = ahora,
            UpdatedAt = ahora
        };
    }

    public void Actualizar(long secuencial, string codigoNumerico)
    {
        Secuencial = secuencial <= 0 ? 1 : secuencial;
        CodigoNumerico = NormalizarCodigoNumerico(codigoNumerico);
        FechaActualizacion = DateTimeOffset.UtcNow;
        UpdatedAt = FechaActualizacion;
    }

    public void Incrementar()
    {
        Secuencial++;
        FechaActualizacion = DateTimeOffset.UtcNow;
        UpdatedAt = FechaActualizacion;
    }

    private static string NormalizarCodigoNumerico(string codigoNumerico) =>
        string.IsNullOrWhiteSpace(codigoNumerico)
            ? "00000000"
            : codigoNumerico.Trim().PadLeft(8, '0')[^8..];
}
