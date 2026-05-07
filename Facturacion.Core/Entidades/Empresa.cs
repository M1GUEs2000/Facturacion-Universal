namespace Facturacion.Core.Entidades;

public class Empresa
{
    protected Empresa() { }

    public string Ruc { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string DirMatriz { get; private set; } = null!;
    public string? NombreComercial { get; private set; }
    public bool ObligadoContabilidad { get; private set; }
    public byte[] CertificadoP12 { get; private set; } = null!;
    public string CertPassword { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Empresa Crear(
        string ruc,
        string nombre,
        string dirMatriz,
        bool obligadoContabilidad,
        byte[] certificadoP12,
        string certPassword,
        string? nombreComercial = null)
    {
        return new Empresa
        {
            Ruc = ruc,
            Nombre = nombre,
            DirMatriz = dirMatriz,
            NombreComercial = nombreComercial,
            ObligadoContabilidad = obligadoContabilidad,
            CertificadoP12 = certificadoP12,
            CertPassword = certPassword,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void ActualizarCertificado(byte[] certificadoP12, string certPassword)
    {
        CertificadoP12 = certificadoP12;
        CertPassword = certPassword;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ActualizarNombre(string nombre)
    {
        Nombre = nombre;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
