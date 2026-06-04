using ErrorOr;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.Cuentas;

/// <summary>
/// Elimina una cuenta aplicando la política GDPR + obligación tributaria de 7 años:
/// - Hard delete: empresas, cert .p12, logo, parametros, secuenciales, logs, docs no fiscales.
/// - Anonimización: documentos fiscales autorizados (razon_social, identificacion, ip) → "ANONIMIZADO".
///   El purge nocturno los borrará físicamente al cumplirse 7 años desde fecha_emision.
/// </summary>
public class EliminarCuenta(
    ICuentasRepositorio cuentas,
    IEmpresasRepositorio empresas,
    IServicioStorage storageDocumentos,
    IServicioStorageFirmaYLogo storageFirmaYLogo)
{
    public async Task<ErrorOr<Deleted>> EjecutarAsync(Guid cuentaId, Guid cuentaJwtId, CancellationToken ct = default)
    {
        if (cuentaId != cuentaJwtId)
            return Errores.Cuenta.Prohibido;

        var cuenta = await cuentas.ObtenerPorIdAsync(cuentaId, ct);
        if (cuenta is null)
            return Errores.Cuenta.NoEncontrada;

        var listaEmpresas = await empresas.ListarPorCuentaAsync(cuentaId, 1, 1000, ct);
        var rucs = listaEmpresas.Select(e => e.Ruc).ToList();

        if (rucs.Count > 0)
        {
            // 1. Eliminar archivos de documentos (XML firmado/autorizado + PDF) del storage (best effort)
            var paths = await cuentas.ObtenerPathsDocumentosPorRucsAsync(rucs, ct);
            foreach (var (xmlFirmado, xmlAutorizado, pdf) in paths)
            {
                if (xmlFirmado is not null)   await storageDocumentos.EliminarAsync(xmlFirmado, ct);
                if (xmlAutorizado is not null) await storageDocumentos.EliminarAsync(xmlAutorizado, ct);
                if (pdf is not null)           await storageDocumentos.EliminarAsync(pdf, ct);
            }

            // 2. Eliminar cert .p12 y logo de cada empresa del storage (best effort)
            foreach (var empresa in listaEmpresas)
            {
                await storageFirmaYLogo.EliminarAsync(RutasStorage.Certificado(empresa.Ruc), ct);
                if (empresa.LogoPath is not null)
                    await storageFirmaYLogo.EliminarAsync(empresa.LogoPath, ct);
            }
        }

        // 3. Anonimizar/eliminar documentos + logs + empresas + cuenta en BD
        await cuentas.EliminarCuentaAsync(cuentaId, rucs, ct);

        return Result.Deleted;
    }
}
