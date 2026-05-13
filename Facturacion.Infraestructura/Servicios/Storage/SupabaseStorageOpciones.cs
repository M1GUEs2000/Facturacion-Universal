namespace Facturacion.Infraestructura.Servicios.Storage;

public class SupabaseStorageOpciones
{
    public const string Seccion = "SupabaseStorage";
    public string Url { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = "documentos";
}
