using System.Xml.Serialization;

namespace Facturacion.Infraestructura.Servicios.Xml.Modelos;

// ─── Compartidos ────────────────────────────────────────────────────────────

public class XmlInfoTributaria
{
    [XmlElement("ambiente")] public string Ambiente { get; set; } = "";
    [XmlElement("tipoEmision")] public string TipoEmision { get; set; } = "1";
    [XmlElement("razonSocial")] public string RazonSocial { get; set; } = "";
    [XmlElement("nombreComercial")] public string? NombreComercial { get; set; }
    public bool ShouldSerializeNombreComercial() => !string.IsNullOrEmpty(NombreComercial);
    [XmlElement("ruc")] public string Ruc { get; set; } = "";
    [XmlElement("claveAcceso")] public string ClaveAcceso { get; set; } = "";
    [XmlElement("codDoc")] public string CodDoc { get; set; } = "";
    [XmlElement("estab")] public string Estab { get; set; } = "";
    [XmlElement("ptoEmi")] public string PtoEmi { get; set; } = "";
    [XmlElement("secuencial")] public string Secuencial { get; set; } = "";
    [XmlElement("dirMatriz")] public string DirMatriz { get; set; } = "";
}

public class XmlTotalImpuesto
{
    [XmlElement("codigo")] public string Codigo { get; set; } = "";
    [XmlElement("codigoPorcentaje")] public string CodigoPorcentaje { get; set; } = "";
    [XmlElement("baseImponible")] public string BaseImponible { get; set; } = "";
    [XmlElement("tarifa")] public string Tarifa { get; set; } = "";
    [XmlElement("valor")] public string Valor { get; set; } = "";
}

public class XmlImpuestoDetalle
{
    [XmlElement("codigo")] public string Codigo { get; set; } = "";
    [XmlElement("codigoPorcentaje")] public string CodigoPorcentaje { get; set; } = "";
    [XmlElement("tarifa")] public string Tarifa { get; set; } = "";
    [XmlElement("baseImponible")] public string BaseImponible { get; set; } = "";
    [XmlElement("valor")] public string Valor { get; set; } = "";
}

public class XmlCampoAdicional
{
    [XmlAttribute("nombre")] public string Nombre { get; set; } = "";
    [XmlText] public string Valor { get; set; } = "";
}

// ─── Factura ────────────────────────────────────────────────────────────────

[XmlRoot("factura")]
public class XmlFactura
{
    [XmlAttribute("id")] public string Id { get; set; } = "comprobante";
    [XmlAttribute("version")] public string Version { get; set; } = "1.1.0";
    [XmlElement("infoTributaria")] public XmlInfoTributaria InfoTributaria { get; set; } = new();
    [XmlElement("infoFactura")] public XmlInfoFactura InfoFactura { get; set; } = new();
    [XmlArray("detalles")] [XmlArrayItem("detalle")] public List<XmlDetalleFactura> Detalles { get; set; } = new();
    [XmlArray("infoAdicional")] [XmlArrayItem("campoAdicional")] public List<XmlCampoAdicional> InfoAdicional { get; set; } = new();
    public bool ShouldSerializeInfoAdicional() => InfoAdicional.Count > 0;
}

public class XmlInfoFactura
{
    [XmlElement("fechaEmision")] public string FechaEmision { get; set; } = "";
    [XmlElement("dirEstablecimiento")] public string? DirEstablecimiento { get; set; }
    public bool ShouldSerializeDirEstablecimiento() => !string.IsNullOrEmpty(DirEstablecimiento);
    [XmlElement("obligadoContabilidad")] public string ObligadoContabilidad { get; set; } = "";
    [XmlElement("tipoIdentificacionComprador")] public string TipoIdentificacionComprador { get; set; } = "";
    [XmlElement("guiaRemision")] public string? GuiaRemision { get; set; }
    public bool ShouldSerializeGuiaRemision() => !string.IsNullOrEmpty(GuiaRemision);
    [XmlElement("razonSocialComprador")] public string RazonSocialComprador { get; set; } = "";
    [XmlElement("identificacionComprador")] public string IdentificacionComprador { get; set; } = "";
    [XmlElement("direccionComprador")] public string? DireccionComprador { get; set; }
    public bool ShouldSerializeDireccionComprador() => !string.IsNullOrEmpty(DireccionComprador);
    [XmlElement("totalSinImpuestos")] public string TotalSinImpuestos { get; set; } = "";
    [XmlElement("totalDescuento")] public string TotalDescuento { get; set; } = "";
    [XmlArray("totalConImpuestos")] [XmlArrayItem("totalImpuesto")] public List<XmlTotalImpuesto> TotalConImpuestos { get; set; } = new();
    [XmlElement("propina")] public string Propina { get; set; } = "";
    [XmlElement("importeTotal")] public string ImporteTotal { get; set; } = "";
    [XmlElement("moneda")] public string Moneda { get; set; } = "DOLAR";
    [XmlArray("pagos")] [XmlArrayItem("pago")] public List<XmlPago> Pagos { get; set; } = new();
}

public class XmlPago
{
    [XmlElement("formaPago")] public string FormaPago { get; set; } = "";
    [XmlElement("total")] public string Total { get; set; } = "";
    [XmlElement("plazo")] public string? Plazo { get; set; }
    public bool ShouldSerializePlazo() => Plazo != null;
    [XmlElement("unidadTiempo")] public string? UnidadTiempo { get; set; }
    public bool ShouldSerializeUnidadTiempo() => !string.IsNullOrEmpty(UnidadTiempo);
}

public class XmlDetalleFactura
{
    [XmlElement("codigoPrincipal")] public string CodigoPrincipal { get; set; } = "";
    [XmlElement("codigoAuxiliar")] public string? CodigoAuxiliar { get; set; }
    public bool ShouldSerializeCodigoAuxiliar() => !string.IsNullOrEmpty(CodigoAuxiliar);
    [XmlElement("descripcion")] public string Descripcion { get; set; } = "";
    [XmlElement("cantidad")] public string Cantidad { get; set; } = "";
    [XmlElement("precioUnitario")] public string PrecioUnitario { get; set; } = "";
    [XmlElement("descuento")] public string Descuento { get; set; } = "";
    [XmlElement("precioTotalSinImpuesto")] public string PrecioTotalSinImpuesto { get; set; } = "";
    [XmlArray("impuestos")] [XmlArrayItem("impuesto")] public List<XmlImpuestoDetalle> Impuestos { get; set; } = new();
}

// ─── Nota de Crédito ────────────────────────────────────────────────────────

[XmlRoot("notaCredito")]
public class XmlNotaCredito
{
    [XmlAttribute("id")] public string Id { get; set; } = "comprobante";
    [XmlAttribute("version")] public string Version { get; set; } = "1.0.0";
    [XmlElement("infoTributaria")] public XmlInfoTributaria InfoTributaria { get; set; } = new();
    [XmlElement("infoNotaCredito")] public XmlInfoNotaCredito InfoNotaCredito { get; set; } = new();
    [XmlArray("detalles")] [XmlArrayItem("detalle")] public List<XmlDetalleFactura> Detalles { get; set; } = new();
    [XmlArray("infoAdicional")] [XmlArrayItem("campoAdicional")] public List<XmlCampoAdicional> InfoAdicional { get; set; } = new();
    public bool ShouldSerializeInfoAdicional() => InfoAdicional.Count > 0;
}

public class XmlInfoNotaCredito
{
    [XmlElement("fechaEmision")] public string FechaEmision { get; set; } = "";
    [XmlElement("dirEstablecimiento")] public string? DirEstablecimiento { get; set; }
    public bool ShouldSerializeDirEstablecimiento() => !string.IsNullOrEmpty(DirEstablecimiento);
    [XmlElement("tipoIdentificacionComprador")] public string TipoIdentificacionComprador { get; set; } = "";
    [XmlElement("razonSocialComprador")] public string RazonSocialComprador { get; set; } = "";
    [XmlElement("identificacionComprador")] public string IdentificacionComprador { get; set; } = "";
    [XmlElement("codDocModificado")] public string CodDocModificado { get; set; } = "";
    [XmlElement("numDocModificado")] public string NumDocModificado { get; set; } = "";
    [XmlElement("fechaEmisionDocSustento")] public string FechaEmisionDocSustento { get; set; } = "";
    [XmlElement("totalSinImpuestos")] public string TotalSinImpuestos { get; set; } = "";
    [XmlElement("valorModificacion")] public string ValorModificacion { get; set; } = "";
    [XmlElement("moneda")] public string Moneda { get; set; } = "DOLAR";
    [XmlArray("totalConImpuestos")] [XmlArrayItem("totalImpuesto")] public List<XmlTotalImpuesto> TotalConImpuestos { get; set; } = new();
    [XmlElement("motivo")] public string Motivo { get; set; } = "";
}

// ─── Retención ──────────────────────────────────────────────────────────────

[XmlRoot("comprobanteRetencion")]
public class XmlRetencion
{
    [XmlAttribute("id")] public string Id { get; set; } = "comprobante";
    [XmlAttribute("version")] public string Version { get; set; } = "1.0.0";
    [XmlElement("infoTributaria")] public XmlInfoTributaria InfoTributaria { get; set; } = new();
    [XmlElement("infoCompRetencion")] public XmlInfoCompRetencion InfoCompRetencion { get; set; } = new();
    [XmlArray("impuestos")] [XmlArrayItem("impuesto")] public List<XmlImpuestoRetencion> Impuestos { get; set; } = new();
    [XmlArray("infoAdicional")] [XmlArrayItem("campoAdicional")] public List<XmlCampoAdicional> InfoAdicional { get; set; } = new();
    public bool ShouldSerializeInfoAdicional() => InfoAdicional.Count > 0;
}

public class XmlInfoCompRetencion
{
    [XmlElement("fechaEmision")] public string FechaEmision { get; set; } = "";
    [XmlElement("dirEstablecimiento")] public string? DirEstablecimiento { get; set; }
    public bool ShouldSerializeDirEstablecimiento() => !string.IsNullOrEmpty(DirEstablecimiento);
    [XmlElement("obligadoContabilidad")] public string ObligadoContabilidad { get; set; } = "";
    [XmlElement("tipoIdentificacionSujetoRetenido")] public string TipoIdentificacionSujeto { get; set; } = "";
    [XmlElement("razonSocialSujetoRetenido")] public string RazonSocialSujeto { get; set; } = "";
    [XmlElement("identificacionSujetoRetenido")] public string IdentificacionSujeto { get; set; } = "";
    [XmlElement("periodoFiscal")] public string PeriodoFiscal { get; set; } = "";
}

public class XmlImpuestoRetencion
{
    [XmlElement("codigo")] public string Codigo { get; set; } = "";
    [XmlElement("codigoRetencion")] public string CodigoRetencion { get; set; } = "";
    [XmlElement("baseImponible")] public string BaseImponible { get; set; } = "";
    [XmlElement("porcentajeRetener")] public string PorcentajeRetener { get; set; } = "";
    [XmlElement("valorRetenido")] public string ValorRetenido { get; set; } = "";
    [XmlElement("codDocSustento")] public string CodDocSustento { get; set; } = "";
    [XmlElement("numDocSustento")] public string NumDocSustento { get; set; } = "";
    [XmlElement("fechaEmisionDocSustento")] public string FechaEmisionDocSustento { get; set; } = "";
}
