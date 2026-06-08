using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Infraestructura.Servicios.Xml.Modelos;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infraestructura.Servicios.Xml;

public class ServicioXml(ILogger<ServicioXml> logger) : IServicioXml
{
    private static readonly XmlSerializerNamespaces EmptyNs = BuildEmptyNs();
    private static readonly ConcurrentDictionary<Type, XmlSerializer> SerializerCache = new();

    private static XmlSerializerNamespaces BuildEmptyNs()
    {
        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        return ns;
    }

    public ErrorOr<string> GenerarXmlFactura(Factura factura, Empresa empresa, ParametrosFacturacion? parametros = null)
    {
        try
        {
            return Serializar(MapearFactura(factura, empresa, parametros));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error serializando XML de factura {ClaveAcceso}", factura.ClaveAcceso);
            return Errores.Xml.ErrorSerializacion;
        }
    }

    public ErrorOr<string> GenerarXmlNotaCredito(NotaCredito notaCredito, Empresa empresa)
    {
        try
        {
            return Serializar(MapearNotaCredito(notaCredito, empresa));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error serializando XML de nota de credito {ClaveAcceso}", notaCredito.ClaveAcceso);
            return Errores.Xml.ErrorSerializacion;
        }
    }

    public ErrorOr<string> GenerarXmlRetencion(Retencion retencion, Empresa empresa, ParametrosFacturacion? parametros = null)
    {
        try
        {
            return Serializar(MapearRetencion(retencion, empresa, parametros));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error serializando XML de retencion {ClaveAcceso}", retencion.ClaveAcceso);
            return Errores.Xml.ErrorSerializacion;
        }
    }

    // ─── Mapping ──────────────────────────────────────────────────────────────

    private static XmlFactura MapearFactura(Factura f, Empresa e, ParametrosFacturacion? p) =>
        new()
        {
            InfoTributaria = BuildInfoTributaria(e, f.ClaveAcceso, TipoDocumentoSri.Factura, f.Ambiente, f.Estab, f.PtoEmi, f.Secuencial),
            InfoFactura = new XmlInfoFactura
            {
                FechaEmision = f.FechaEmision.ToString("dd/MM/yyyy"),
                DirEstablecimiento = f.DirEstablecimiento,
                ObligadoContabilidad = (p?.ObligadoContabilidad ?? false) ? "SI" : "NO",
                TipoIdentificacionComprador = f.TipoIdentificacionComprador,
                RazonSocialComprador = f.RazonSocialComprador,
                IdentificacionComprador = f.IdentificacionComprador,
                DireccionComprador = f.DireccionComprador,
                Moneda = ConstantesSri.Moneda,
                GuiaRemision = f.GuiaRemision,
                TotalSinImpuestos = M(f.TotalSinImpuestos),
                TotalDescuento = M(f.TotalDescuento),
                TotalConImpuestos = BuildTotalImpuestosFactura(f),
                Propina = M(f.Propina),
                ImporteTotal = M(f.ImporteTotal),
                Pagos = f.FormasPago.Select(fp => new XmlPago
                {
                    FormaPago = fp.Codigo,
                    Total = M(fp.Total),
                    Plazo = fp.Plazo?.ToString(),
                    UnidadTiempo = fp.UnidadTiempo
                }).ToList()
            },
            Detalles = f.Detalle.OrderBy(d => d.Orden).Select(MapDetalleFactura).ToList(),
            InfoAdicional = f.InfoAdicional.Select(MapCampoAdicional).ToList()
        };

    private static XmlNotaCredito MapearNotaCredito(NotaCredito n, Empresa e) =>
        new()
        {
            InfoTributaria = BuildInfoTributaria(e, n.ClaveAcceso, TipoDocumentoSri.NotaCredito, n.Ambiente, n.Estab, n.PtoEmi, n.Secuencial),
            InfoNotaCredito = new XmlInfoNotaCredito
            {
                FechaEmision = n.FechaEmision.ToString("dd/MM/yyyy"),
                DirEstablecimiento = n.DirEstablecimiento,
                TipoIdentificacionComprador = n.TipoIdentificacionComprador,
                RazonSocialComprador = n.RazonSocialComprador,
                IdentificacionComprador = n.IdentificacionComprador,
                CodDocModificado = n.DocModificadoTipo,
                NumDocModificado = n.DocModificadoNumero,
                FechaEmisionDocSustento = n.DocModificadoFecha.ToString("dd/MM/yyyy"),
                TotalSinImpuestos = M(n.TotalSinImpuestos),
                ValorModificacion = M(n.ValorModificacion),
                Moneda = ConstantesSri.Moneda,
                TotalConImpuestos = BuildTotalImpuestosNota(n),
                Motivo = n.Motivo
            },
            Detalles = n.Detalle.OrderBy(d => d.Orden).Select(MapDetalleNota).ToList(),
            InfoAdicional = n.InfoAdicional.Select(MapCampoAdicional).ToList()
        };

    private static XmlRetencion MapearRetencion(Retencion r, Empresa e, ParametrosFacturacion? p) =>
        new()
        {
            InfoTributaria = BuildInfoTributaria(e, r.ClaveAcceso, TipoDocumentoSri.Retencion, r.Ambiente, r.Estab, r.PtoEmi, r.Secuencial),
            InfoCompRetencion = new XmlInfoCompRetencion
            {
                FechaEmision = r.FechaEmision.ToString("dd/MM/yyyy"),
                ObligadoContabilidad = (p?.ObligadoContabilidad ?? false) ? "SI" : "NO",
                TipoIdentificacionSujeto = r.TipoIdentificacionSujeto,
                RazonSocialSujeto = r.RazonSocialSujeto,
                IdentificacionSujeto = r.IdentificacionSujeto,
                PeriodoFiscal = r.PeriodoFiscal
            },
            Impuestos = r.Detalle.OrderBy(d => d.Orden).Select(d => new XmlImpuestoRetencion
            {
                Codigo = d.CodigoImpuesto,
                CodigoRetencion = d.CodigoRetencion,
                BaseImponible = M(d.BaseImponible),
                PorcentajeRetener = M(d.PorcentajeRetener),
                ValorRetenido = M(d.ValorRetenido),
                CodDocSustento = d.CodDocSustento,
                NumDocSustento = d.NumDocSustento,
                FechaEmisionDocSustento = d.FechaEmisionDocSustento.ToString("dd/MM/yyyy")
            }).ToList(),
            InfoAdicional = r.InfoAdicional.Select(MapCampoAdicional).ToList()
        };

    // ─── Builders ─────────────────────────────────────────────────────────────

    private static XmlInfoTributaria BuildInfoTributaria(
        Empresa e, string claveAcceso, string codDoc,
        Ambiente ambiente, string estab, string ptoEmi, string secuencial) =>
        new()
        {
            Ambiente = ((int)ambiente).ToString(),
            TipoEmision = ConstantesSri.TipoEmisionNormal,
            RazonSocial = e.Nombre,
            NombreComercial = e.NombreComercial,
            Ruc = e.Ruc,
            ClaveAcceso = claveAcceso,
            CodDoc = codDoc,
            Estab = estab,
            PtoEmi = ptoEmi,
            Secuencial = secuencial,
            DirMatriz = e.DirMatriz
        };

    private static List<XmlTotalImpuesto> BuildTotalImpuestosFactura(Factura f) =>
        BuildTotalImpuestos(f.Detalle,
            d => d.IceCodigo, d => d.IceTarifa, d => d.IceBase, d => d.IceValor,
            d => d.IvaCodigo, d => d.IvaTarifa, d => d.IvaBase, d => d.IvaValor);

    private static List<XmlTotalImpuesto> BuildTotalImpuestosNota(NotaCredito n) =>
        BuildTotalImpuestos(n.Detalle,
            d => d.IceCodigo, d => d.IceTarifa, d => d.IceBase, d => d.IceValor,
            d => d.IvaCodigo, d => d.IvaTarifa, d => d.IvaBase, d => d.IvaValor);

    private static List<XmlTotalImpuesto> BuildTotalImpuestos<T>(
        IEnumerable<T> detalle,
        Func<T, string?> getIceCodigo,
        Func<T, decimal?> getIceTarifa,
        Func<T, decimal?> getIceBase,
        Func<T, decimal?> getIceValor,
        Func<T, CodigoIva> getIvaCodigo,
        Func<T, decimal> getIvaTarifa,
        Func<T, decimal> getIvaBase,
        Func<T, decimal> getIvaValor)
    {
        var lista = new List<XmlTotalImpuesto>();
        var detalleList = detalle.ToList();

        // ICE (código 3) — primero, requerido antes del IVA por el SRI
        foreach (var g in detalleList.Where(d => getIceCodigo(d) != null).GroupBy(getIceCodigo))
            lista.Add(new XmlTotalImpuesto
            {
                Codigo = CodigoImpuestoSri.Ice,
                CodigoPorcentaje = g.Key!,
                BaseImponible = M(g.Sum(d => getIceBase(d) ?? 0m)),
                Tarifa = M(getIceTarifa(g.First()) ?? 0m),
                Valor = M(g.Sum(d => getIceValor(d) ?? 0m))
            });

        // IVA (código 2) — segundo
        foreach (var g in detalleList.GroupBy(getIvaCodigo))
            lista.Add(new XmlTotalImpuesto
            {
                Codigo = CodigoImpuestoSri.Iva,
                CodigoPorcentaje = ((int)g.Key).ToString(),
                BaseImponible = M(g.Sum(getIvaBase)),
                Tarifa = M(getIvaTarifa(g.First())),
                Valor = M(g.Sum(getIvaValor))
            });

        return lista;
    }

    private static XmlDetalleFactura MapDetalleFactura(FacturaDetalle d) =>
        new()
        {
            CodigoPrincipal = d.CodigoPrincipal,
            CodigoAuxiliar = d.CodigoAuxiliar,
            Descripcion = d.Descripcion,
            Cantidad = M(d.Cantidad),
            PrecioUnitario = PU(d.PrecioUnitario),
            Descuento = M(d.Descuento),
            PrecioTotalSinImpuesto = M(d.PrecioTotalSinImpuesto),
            Impuestos = BuildImpuestosDetalle(d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
                d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor)
        };

    private static XmlDetalleFactura MapDetalleNota(NotaCreditoDetalle d) =>
        new()
        {
            CodigoPrincipal = d.CodigoPrincipal,
            CodigoAuxiliar = d.CodigoAuxiliar,
            Descripcion = d.Descripcion,
            Cantidad = M(d.Cantidad),
            PrecioUnitario = PU(d.PrecioUnitario),
            Descuento = M(d.Descuento),
            PrecioTotalSinImpuesto = M(d.PrecioTotalSinImpuesto),
            Impuestos = BuildImpuestosDetalle(d.IceCodigo, d.IceTarifa, d.IceBase, d.IceValor,
                d.IvaCodigo, d.IvaTarifa, d.IvaBase, d.IvaValor)
        };

    private static List<XmlImpuestoDetalle> BuildImpuestosDetalle(
        string? iceCodigo, decimal? iceTarifa, decimal? iceBase, decimal? iceValor,
        CodigoIva ivaCodigo, decimal ivaTarifa, decimal ivaBase, decimal ivaValor)
    {
        var iva = new XmlImpuestoDetalle
        {
            Codigo = "2",
            CodigoPorcentaje = ((int)ivaCodigo).ToString(),
            Tarifa = M(ivaTarifa),
            BaseImponible = M(ivaBase),
            Valor = M(ivaValor)
        };
        if (iceCodigo is null)
            return [iva];
        return [new XmlImpuestoDetalle
        {
            Codigo = "3",
            CodigoPorcentaje = iceCodigo,
            Tarifa = M(iceTarifa ?? 0m),
            BaseImponible = M(iceBase ?? 0m),
            Valor = M(iceValor ?? 0m)
        }, iva];
    }

    private static XmlCampoAdicional MapCampoAdicional(InfoAdicional ia) =>
        new() { Nombre = ia.Nombre, Valor = ia.Valor };

    // ─── Formateo ─────────────────────────────────────────────────────────────

    private static string M(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
    private static string PU(decimal v) => v.ToString("0.000000", CultureInfo.InvariantCulture);

    // ─── Serialización ────────────────────────────────────────────────────────

    private static string Serializar<T>(T modelo)
    {
        var serializer = SerializerCache.GetOrAdd(typeof(T), t => new XmlSerializer(t));
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = false,
            OmitXmlDeclaration = false
        };

        using var ms = new MemoryStream();
        using var writer = XmlWriter.Create(ms, settings);
        serializer.Serialize(writer, modelo, EmptyNs);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
