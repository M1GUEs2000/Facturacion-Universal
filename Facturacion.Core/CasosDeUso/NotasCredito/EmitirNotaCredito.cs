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
    string? Secuencial,
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
    string? IpAddress = null,
    Guid? CuentaId = null);

public class EmitirNotaCredito(
    IEmpresasRepositorio empresas,
    INotasCreditoRepositorio notasCredito,
    IParametrosFacturacionRepositorio parametrosRepo,
    ISecuencialesSriRepositorio secuenciales,
    IServicioXml xml,
    IServicioPdf pdf,
    IServicioStorageFirmaYLogo storageFirma,
    OrquestadorEmision orquestador)
{
    public async Task<ErrorOr<NotaCredito>> EjecutarAsync(ComandoEmitirNotaCredito cmd, CancellationToken ct = default)
    {
        var empresa = await empresas.ObtenerPorRucAsync(cmd.EmpresaRuc, ct);
        if (empresa is null)
            return Errores.Empresa.NoEncontrada;

        if (cmd.CuentaId.HasValue && empresa.CuentaId != cmd.CuentaId.Value)
            return Errores.Empresa.Prohibido;

        var certResult = await storageFirma.ObtenerAsync(empresa.CertificadoPath, ct);
        if (certResult.IsError) return certResult.Errors;

        string secuencial;
        if (cmd.Secuencial is not null)
        {
            if (await notasCredito.ExisteSecuencialActivoAsync(empresa.Ruc, cmd.Estab, cmd.PtoEmi, cmd.Secuencial, cmd.Ambiente, ct))
                return Errores.NotaCredito.SecuencialDuplicado;
            secuencial = cmd.Secuencial;
        }
        else
        {
            var secResult = await secuenciales.IncrementarYObtenerAsync(cmd.EmpresaRuc, TipoDocumentoSri.NotaCredito, ct);
            if (secResult.IsError) return secResult.Errors;
            secuencial = secResult.Value.ToString("D9");
        }

        var claveAcceso = GeneradorClaveAcceso.Generar(
            cmd.FechaEmision, TipoDocumentoSri.NotaCredito, empresa.Ruc,
            cmd.Ambiente, cmd.Estab, cmd.PtoEmi, secuencial);

        var parametros = await parametrosRepo.ObtenerPorEmpresaAsync(cmd.EmpresaRuc, ct);

        byte[]? logoBytes = null;
        if (empresa.LogoPath is not null)
        {
            var logoResult = await storageFirma.ObtenerAsync(empresa.LogoPath, ct);
            if (!logoResult.IsError) logoBytes = logoResult.Value;
        }

        var notaId = Guid.NewGuid();
        var detalle = cmd.Detalle.Select(d => NotaCreditoDetalle.Crear(
            notaId, d.Orden, d.CodigoPrincipal, d.CodigoAuxiliar, d.Descripcion,
            d.Cantidad, d.PrecioUnitario, d.Descuento, d.PrecioTotalSinImpuesto,
            d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
            d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor)).ToList();

        var nota = NotaCredito.Crear(
            cmd.EmpresaRuc, cmd.Ambiente, cmd.Estab, cmd.PtoEmi, secuencial, claveAcceso,
            cmd.FechaEmision, cmd.TipoIdentificacionComprador, cmd.IdentificacionComprador,
            cmd.RazonSocialComprador, cmd.DireccionComprador, cmd.DirEstablecimiento,
            cmd.DocModificadoTipo, cmd.DocModificadoNumero, cmd.DocModificadoFecha, cmd.DocModificadoClaveAcceso,
            cmd.Motivo, cmd.TotalSinImpuestos, cmd.TotalDescuento, cmd.BaseImponibleIce, cmd.ValorIce,
            cmd.BaseImponibleIva, cmd.ValorIva, cmd.ValorModificacion,
            cmd.InfoAdicional, detalle,
            ipAddress: cmd.IpAddress, id: notaId);

        var xmlResult = xml.GenerarXmlNotaCredito(nota, empresa);
        if (xmlResult.IsError) return xmlResult.Errors;

        await notasCredito.AgregarAsync(nota, ct);

        return await orquestador.EjecutarAsync(new ParametrosEmision<NotaCredito>(
            nota, claveAcceso, cmd.Ambiente, xmlResult.Value,
            RutasStorage.PrefijoNotasCredito(empresa.Ruc),
            certResult.Value, empresa.CertPassword,
            (n, t) => pdf.GenerarRideNotaCreditoAsync(n, empresa, parametros, logoBytes, t)), ct);
    }
}
