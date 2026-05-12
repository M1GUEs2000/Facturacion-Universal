namespace Facturacion.Core.Entidades;

public class Empresa
{
    protected Empresa() { }

    public string Ruc { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string DirMatriz { get; private set; } = null!;
    public string? NombreComercial { get; private set; }
    public byte[]? Logo { get; private set; }
    public string? LogoContentType { get; private set; }
    public byte[] CertificadoP12 { get; private set; } = null!;
    public string CertPassword { get; private set; } = null!;
    public Guid CuentaId { get; private set; }
    public Cuenta? Cuenta { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Empresa Crear(
        string ruc,
        string nombre,
        string dirMatriz,
        byte[] certificadoP12,
        string certPassword,
        Guid cuentaId,
        string? nombreComercial = null,
        byte[]? logo = null,
        string? logoContentType = null)
    {
        return new Empresa
        {
            Ruc = ruc,
            Nombre = nombre,
            DirMatriz = dirMatriz,
            NombreComercial = nombreComercial,
            Logo = logo,
            LogoContentType = logoContentType,
            CertificadoP12 = certificadoP12,
            CertPassword = certPassword,
            CuentaId = cuentaId,
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

    public void ActualizarLogo(byte[]? logo, string? contentType)
    {
        Logo = logo;
        LogoContentType = contentType;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
