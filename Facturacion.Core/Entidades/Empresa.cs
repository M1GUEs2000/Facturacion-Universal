namespace Facturacion.Core.Entidades;

public class Empresa
{
    protected Empresa() { }

    public string Ruc { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string DirMatriz { get; private set; } = null!;
    public string? NombreComercial { get; private set; }
    public string? LogoPath { get; private set; }
    public string? LogoContentType { get; private set; }
    public string CertificadoPath { get; private set; } = null!;
    public string CertPassword { get; private set; } = null!;
    public Guid CuentaId { get; private set; }
    public Cuenta? Cuenta { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Empresa Crear(
        string ruc,
        string nombre,
        string dirMatriz,
        string certificadoPath,
        string certPassword,
        Guid cuentaId,
        string? nombreComercial = null,
        string? logoPath = null,
        string? logoContentType = null)
    {
        return new Empresa
        {
            Ruc = ruc,
            Nombre = nombre,
            DirMatriz = dirMatriz,
            NombreComercial = nombreComercial,
            LogoPath = logoPath,
            LogoContentType = logoContentType,
            CertificadoPath = certificadoPath,
            CertPassword = certPassword,
            CuentaId = cuentaId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void ActualizarCertificado(string certificadoPath, string certPassword)
    {
        CertificadoPath = certificadoPath;
        CertPassword = certPassword;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ActualizarNombre(string nombre)
    {
        Nombre = nombre;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ActualizarDatos(
        string nombre,
        string dirMatriz,
        string? nombreComercial = null)
    {
        Nombre = nombre;
        DirMatriz = dirMatriz;
        NombreComercial = nombreComercial;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ActualizarLogo(string? logoPath, string? contentType)
    {
        LogoPath = logoPath;
        LogoContentType = contentType;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
