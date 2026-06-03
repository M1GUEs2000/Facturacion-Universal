using ErrorOr;
using Facturacion.Core.Interfaces;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;

namespace Facturacion.Core.CasosDeUso.Comun;

public enum TipoArchivoDescarga { Pdf, Xml }

public record UrlDescargaDocumento(string Url, DateTimeOffset ExpiresAt);

public class ObtenerUrlDescarga(IEmpresasRepositorio empresas, IServicioStorage storage)
{
    private const int TtlSegundos = 300;

    public async Task<ErrorOr<UrlDescargaDocumento>> EjecutarAsync(
        IDocumentoEmitible doc,
        TipoArchivoDescarga tipo,
        Guid cuentaId,
        CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(doc.EmpresaRuc, ct);
        if (empresa is null) return Errores.Empresa.NoEncontrada;
        if (empresa.CuentaId != cuentaId) return Errores.Empresa.Prohibido;

        var ruta = tipo == TipoArchivoDescarga.Pdf ? doc.PdfPath : doc.XmlAutorizadoPath;
        if (ruta is null)
            return tipo == TipoArchivoDescarga.Pdf
                ? Errores.Documento.SinPdf
                : Errores.Documento.SinXml;

        var urlResult = await storage.GenerarUrlFirmadaAsync(ruta, TtlSegundos, ct);
        if (urlResult.IsError) return urlResult.Errors;

        return new UrlDescargaDocumento(urlResult.Value, DateTimeOffset.UtcNow.AddSeconds(TtlSegundos));
    }
}
