namespace Facturacion.Core.Interfaces;

public interface IAuditLogger
{
    void Registrar(EventoAudit evento);
}

public record EventoAudit(
    string Tipo,
    Guid CuentaId,
    string? Ruc = null,
    string? ClaveAcceso = null,
    string? Ip = null,
    bool Exito = true,
    string? CodigoError = null);

public static class EventosAudit
{
    public const string FacturaEmitida         = "Factura.Emitida";
    public const string FacturaReintentada     = "Factura.ReintentadaEmision";
    public const string NotaCreditoEmitida     = "NotaCredito.Emitida";
    public const string NotaCreditoReintentada = "NotaCredito.ReintentadaEmision";
    public const string RetencionEmitida       = "Retencion.Emitida";
    public const string RetencionReintentada   = "Retencion.ReintentadaEmision";
    public const string EmpresaRegistrada      = "Empresa.Registrada";
    public const string EmpresaActualizada     = "Empresa.Actualizada";
    public const string CertificadoActualizado = "Empresa.CertificadoActualizado";
    public const string CuentaEliminada        = "Cuenta.Eliminada";
}
