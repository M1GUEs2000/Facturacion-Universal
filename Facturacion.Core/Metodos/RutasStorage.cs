namespace Facturacion.Core.Metodos;

/// <summary>
/// Rutas de almacenamiento para documentos electrónicos.
/// Cambiar aquí afecta todos los emisores y reintentos de los tres tipos de comprobante.
/// </summary>
public static class RutasStorage
{
    // ── Prefijos de carpeta por tipo de comprobante ──────────────────────────

    public static string PrefijoFacturas(string ruc) => $"{ruc}/facturas";
    public static string PrefijoNotasCredito(string ruc) => $"{ruc}/notas-credito";
    public static string PrefijoRetenciones(string ruc) => $"{ruc}/retenciones";

    // ── Rutas de archivo ──────────────────────────────────────────────────────

    public static string XmlFirmado(string prefijo, string claveAcceso) =>
        $"{prefijo}/{claveAcceso}.xml";

    public static string XmlAutorizado(string prefijo, string claveAcceso) =>
        $"{prefijo}/{claveAcceso}_autorizado.xml";

    public static string Pdf(string prefijo, string claveAcceso) =>
        $"{prefijo}/{claveAcceso}.pdf";

    // ── Rutas de archivos de empresa ─────────────────────────────────────────

    public static string Certificado(string ruc) => $"{ruc}/certificado.p12";
    public static string Logo(string ruc) => $"{ruc}/logo";
}
