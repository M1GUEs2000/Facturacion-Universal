using ErrorOr;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;

namespace Facturacion.Core.CasosDeUso.NotasCredito;

public record ComandoDetalleNotaCredito(
    int Orden,
    string CodigoPrincipal,
    string? CodigoAuxiliar,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Descuento,
    decimal PrecioTotalSinImpuesto,
    string? IceCodigo,
    decimal? IceTarifa,
    decimal? IceBase,
    decimal? IceValor,
    CodigoIva IvaCodigo,
    decimal IvaTarifa,
    decimal IvaBase,
    decimal IvaValor);

public record ComandoEmitirNotaCredito(
    string EmpresaRuc,
    Ambiente Ambiente,
    string Estab,
    string PtoEmi,
    string Secuencial,
    DateOnly FechaEmision,
    string TipoIdentificacionComprador,
    string IdentificacionComprador,
    string RazonSocialComprador,
    string? DireccionComprador,
    string? DirEstablecimiento,
    string DocModificadoTipo,
    string DocModificadoNumero,
    DateOnly DocModificadoFecha,
    string DocModificadoClaveAcceso,
    string Motivo,
    decimal TotalSinImpuestos,
    decimal TotalDescuento,
    decimal? BaseImponibleIce,
    decimal? ValorIce,
    decimal BaseImponibleIva,
    decimal ValorIva,
    decimal ValorModificacion,
    List<InfoAdicional> InfoAdicional,
    List<ComandoDetalleNotaCredito> Detalle,
    string? IpAddress = null);

public class EmitirNotaCredito(
    IEmpresasRepositorio empresas,
    INotasCreditoRepositorio notasCredito,
    IServicioXml xml,
    IServicioPdf pdf,
    OrquestadorEmision orquestador)
{
    public async Task<ErrorOr<NotaCredito>> EjecutarAsync(ComandoEmitirNotaCredito cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.EmpresaRuc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.NotaCredito, empresa.Ruc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial);

        if (await notasCredito.ExisteClaveAccesoAsync(claveAcceso, ct))
            return Errores.NotaCredito.ClaveAccesoDuplicada;

        var notaId = Guid.NewGuid();
        var detalle = cmd.Detalle.Select(d => NotaCreditoDetalle.Crear(
            notaId, d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
            d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
            d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
            d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor)).ToList();

        var nota = NotaCredito.Crear(
            cmd.EmpresaRuc, cmd.Ambiente, cmd.Estab, cmd.PtoEmi, cmd.Secuencial, claveAcceso,
            cmd.FechaEmision, cmd.TipoIdentificacionComprador, cmd.IdentificacionComprador,
            cmd.RazonSocialComprador, cmd.DireccionComprador, cmd.DirEstablecimiento,
            cmd.DocModificadoTipo, cmd.DocModificadoNumero, cmd.DocModificadoFecha, cmd.DocModificadoClaveAcceso,
            cmd.Motivo, cmd.TotalSinImpuestos, cmd.TotalDescuento, cmd.BaseImponibleIce, cmd.ValorIce,
            cmd.BaseImponibleIva, cmd.ValorIva, cmd.ValorModificacion,
            cmd.InfoAdicional, detalle, cmd.IpAddress);

        var xmlResult = xml.GenerarXmlNotaCredito(nota, empresa);
        if (xmlResult.IsError) return xmlResult.Errors;

        return await orquestador.EjecutarAsync(new ParametrosEmision<NotaCredito>(
            nota, claveAcceso, cmd.Ambiente, xmlResult.Value,
            $"{empresa.Ruc}/notas-credito",
            empresa.CertificadoP12, empresa.CertPassword,
            (n, t) => pdf.GenerarRideNotaCreditoAsync(n, t),
            (n, t) => notasCredito.AgregarAsync(n, t)), ct);
    }
}
