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

namespace Facturacion.Infraestructura.Servicios.Xml;

public class ServicioXml : IServicioXml
{
    private static readonly XmlSerializerNamespaces EmptyNs = BuildEmptyNs();

    private static XmlSerializerNamespaces BuildEmptyNs()
    {
        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        return ns;
    }

    public ErrorOr<string> GenerarXmlFactura(Factura factura, Empresa empresa)
    {
        try
        {
            return Serializar(MapearFactura(factura, empresa));
        }
        catch (Exception)
        {
            return Errores.Xml.ErrorSerializacion;
        }
    }

    public ErrorOr<string> GenerarXmlNotaCredito(NotaCredito notaCredito, Empresa empresa)
    {
        try
        {
            return Serializar(MapearNotaCredito(notaCredito, empresa));
        }
        catch (Exception)
        {
            return Errores.Xml.ErrorSerializacion;
        }
    }

    public ErrorOr<string> GenerarXmlRetencion(Retencion retencion, Empresa empresa)
    {
        try
        {
            return Serializar(MapearRetencion(retencion, empresa));
        }
        catch (Exception)
        {
            return Errores.Xml.ErrorSerializacion;
        }
    }

    // ─── Mapping ──────────────────────────────────────────────────────────────

    private static XmlFactura MapearFactura(Factura f, Empresa e) =>
        new()
        {
            InfoTributaria = BuildInfoTributaria(e, f.ClaveAcceso, "01", f.Ambiente, f.Estab, f.PtoEmi, f.Secuencial),
            InfoFactura = new XmlInfoFactura
            {
                FechaEmision = f.FechaEmision.ToString("dd/MM/yyyy"),
                DirEstablecimiento = f.DirEstablecimiento,
                ObligadoContabilidad = e.ObligadoContabilidad ? "SI" : "NO",
                TipoIdentificacionComprador = f.TipoIdentificacionComprador,
                RazonSocialComprador = f.RazonSocialComprador,
                IdentificacionComprador = f.IdentificacionComprador,
                DireccionComprador = f.DireccionComprador,
                Moneda = "DOLAR",
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
            InfoTributaria = BuildInfoTributaria(e, n.ClaveAcceso, "04", n.Ambiente, n.Estab, n.PtoEmi, n.Secuencial),
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
                Moneda = "DOLAR",
                TotalConImpuestos = BuildTotalImpuestosNota(n),
                Motivo = n.Motivo
            },
            Detalles = n.Detalle.OrderBy(d => d.Orden).Select(MapDetalleNota).ToList(),
            InfoAdicional = n.InfoAdicional.Select(MapCampoAdicional).ToList()
        };

    private static XmlRetencion MapearRetencion(Retencion r, Empresa e) =>
        new()
        {
            InfoTributaria = BuildInfoTributaria(e, r.ClaveAcceso, "07", r.Ambiente, r.Estab, r.PtoEmi, r.Secuencial),
            InfoCompRetencion = new XmlInfoCompRetencion
            {
                FechaEmision = r.FechaEmision.ToString("dd/MM/yyyy"),
                ObligadoContabilidad = e.ObligadoContabilidad ? "SI" : "NO",
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
            TipoEmision = "1",
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

    private static List<XmlTotalImpuesto> BuildTotalImpuestosFactura(Factura f)
    {
        var lista = new List<XmlTotalImpuesto>();

        // ICE (código 3) — primero, requerido antes del IVA por el SRI
        foreach (var g in f.Detalle.Where(d => d.IceCodigo != null).GroupBy(d => d.IceCodigo!))
            lista.Add(new XmlTotalImpuesto
            {
                Codigo = "3",
                CodigoPorcentaje = g.Key,
                BaseImponible = M(g.Sum(d => d.IceBase ?? 0m)),
                Tarifa = M(g.First().IceTarifa ?? 0m),
                Valor = M(g.Sum(d => d.IceValor ?? 0m))
            });

        // IVA (código 2) — segundo
        foreach (var g in f.Detalle.GroupBy(d => d.IvaCodigo))
            lista.Add(new XmlTotalImpuesto
            {
                Codigo = "2",
                CodigoPorcentaje = ((int)g.Key).ToString(),
                BaseImponible = M(g.Sum(d => d.IvaBase)),
                Tarifa = M(g.First().IvaTarifa),
                Valor = M(g.Sum(d => d.IvaValor))
            });

        return lista;
    }

    private static List<XmlTotalImpuesto> BuildTotalImpuestosNota(NotaCredito n)
    {
        var lista = new List<XmlTotalImpuesto>();

        foreach (var g in n.Detalle.Where(d => d.IceCodigo != null).GroupBy(d => d.IceCodigo!))
            lista.Add(new XmlTotalImpuesto
            {
                Codigo = "3",
                CodigoPorcentaje = g.Key,
                BaseImponible = M(g.Sum(d => d.IceBase ?? 0m)),
                Tarifa = M(g.First().IceTarifa ?? 0m),
                Valor = M(g.Sum(d => d.IceValor ?? 0m))
            });

        foreach (var g in n.Detalle.GroupBy(d => d.IvaCodigo))
            lista.Add(new XmlTotalImpuesto
            {
                Codigo = "2",
                CodigoPorcentaje = ((int)g.Key).ToString(),
                BaseImponible = M(g.Sum(d => d.IvaBase)),
                Tarifa = M(g.First().IvaTarifa),
                Valor = M(g.Sum(d => d.IvaValor))
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
        var lista = new List<XmlImpuestoDetalle>();

        if (iceCodigo != null)
            lista.Add(new XmlImpuestoDetalle
            {
                Codigo = "3",
                CodigoPorcentaje = iceCodigo,
                Tarifa = M(iceTarifa ?? 0m),
                BaseImponible = M(iceBase ?? 0m),
                Valor = M(iceValor ?? 0m)
            });

        lista.Add(new XmlImpuestoDetalle
        {
            Codigo = "2",
            CodigoPorcentaje = ((int)ivaCodigo).ToString(),
            Tarifa = M(ivaTarifa),
            BaseImponible = M(ivaBase),
            Valor = M(ivaValor)
        });

        return lista;
    }

    private static XmlCampoAdicional MapCampoAdicional(InfoAdicional ia) =>
        new() { Nombre = ia.Nombre, Valor = ia.Valor };

    // ─── Formateo ─────────────────────────────────────────────────────────────

    private static string M(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
    private static string PU(decimal v) => v.ToString("0.000000", CultureInfo.InvariantCulture);

    // ─── Serialización ────────────────────────────────────────────────────────

    private static string Serializar<T>(T modelo)
    {
        var serializer = new XmlSerializer(typeof(T));
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

