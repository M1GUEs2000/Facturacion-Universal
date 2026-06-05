using Facturacion.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infraestructura.Audit;

public class AuditLogger(ILogger<AuditLogger> logger) : IAuditLogger
{
    public void Registrar(EventoAudit evento)
    {
        logger.LogInformation("[AUDIT] {@Audit}", evento);
    }
}
