namespace Facturacion.Infraestructura.Servicios.Storage;

public class SupabaseStorageFirmaYLogoOpciones
{
    public const string Seccion = "SupabaseStorageFirmaYLogo";
    public string Url { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = "firmaylogo";
}
