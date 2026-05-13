using ErrorOr;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.Facturas;

public class ReintentarEmisionFactura(
    IEmpresasRepositorio empresas,
    IFacturasRepositorio facturas,
    IParametrosFacturacionRepositorio parametrosRepo,
    IServicioXml xml,
    IServicioPdf pdf,
    IServicioStorageFirmaYLogo storageFirma,
    OrquestadorReintento orquestador)
{
    public async Task<ErrorOr<Factura>> EjecutarAsync(Guid facturaId, CancellationToken ct = default)
    {
        var factura = await facturas.ObtenerPorIdAsync(facturaId, ct);
        if (factura is null) return Errores.Factura.NoEncontrada;

        if (factura.EstadoSri is Enums.EstadoSri.Autorizado or Enums.EstadoSri.NoAutorizado or Enums.EstadoSri.Anulado)
            return Errores.Factura.EstadoInvalido;

        var empresa = await empresas.ObtenerPorRucAsync(factura.EmpresaRuc, ct);
        if (empresa is null) return Errores.Empresa.NoEncontrada;

        var certResult = await storageFirma.ObtenerAsync(empresa.CertificadoPath, ct);
        if (certResult.IsError) return certResult.Errors;

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(factura.EmpresaRuc, ct);

        byte[]? logoBytes = null;
        if (empresa.LogoPath is not null)
        {
            var logoResult = await storageFirma.ObtenerAsync(empresa.LogoPath, ct);
            if (!logoResult.IsError) logoBytes = logoResult.Value;
        }

        return await orquestador.EjecutarAsync(new ParametrosReintento<Factura>(
            factura,
            factura.ClaveAcceso,
            factura.Ambiente,
            RutasStorage.PrefijoFacturas(empresa.Ruc),
            certResult.Value,
            empresa.CertPassword,
            (f, _) => xml.GenerarXmlFactura(f, empresa, parametros),
            (f, t) => pdf.GenerarRideFacturaAsync(f, empresa, parametros, logoBytes, t),
            (f, t) => facturas.ActualizarAsync(f, t)), ct);
    }
}
