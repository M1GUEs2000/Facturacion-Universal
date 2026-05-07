using ErrorOr;
using Facturacion.Core.Entidades;

namespace Facturacion.Core.Interfaces.Servicios;

public interface IServicioXml
{
    ErrorOr<string> GenerarXmlFactura(Factura factura, Empresa empresa);
    ErrorOr<string> GenerarXmlNotaCredito(NotaCredito notaCredito, Empresa empresa);
    ErrorOr<string> GenerarXmlRetencion(Retencion retencion, Empresa empresa);
}
