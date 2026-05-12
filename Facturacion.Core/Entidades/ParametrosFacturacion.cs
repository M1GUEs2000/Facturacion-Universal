using Facturacion.Core.Enums;

namespace Facturacion.Core.Entidades;

public class ParametrosFacturacion
{
    protected ParametrosFacturacion() { }

    public string EmpresaRuc { get; private set; } = null!;
    public Ambiente Ambiente { get; private set; }
    public string TipoEmision { get; private set; } = null!;
    public bool AgenteRetencion { get; private set; }
    public string? ContribuyenteRimpe { get; private set; }
    public string Estab { get; private set; } = null!;
    public string PuntoEmision { get; private set; } = null!;
    public string? ContribuyenteEspecial { get; private set; }
    public bool ObligadoContabilidad { get; private set; }
    public string Moneda { get; private set; } = null!;
    public string CodigoImpuesto { get; private set; } = null!;
    public CodigoIva CodigoPorcentaje { get; private set; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ParametrosFacturacion Crear(
        string empresaRuc,
        Ambiente ambiente,
        string tipoEmision,
        bool agenteRetencion,
        string? contribuyenteRimpe,
        string estab,
        string puntoEmision,
        string? contribuyenteEspecial,
        bool obligadoContabilidad,
        string moneda,
        string codigoImpuesto,
        CodigoIva codigoPorcentaje)
    {
        var ahora = DateTimeOffset.UtcNow;
        return new ParametrosFacturacion
        {
            EmpresaRuc = empresaRuc,
            CreatedAt = ahora,
            UpdatedAt = ahora,
            FechaActualizacion = ahora
        }.Aplicar(
            ambiente, tipoEmision, agenteRetencion, contribuyenteRimpe,
            estab, puntoEmision, contribuyenteEspecial, obligadoContabilidad,
            moneda, codigoImpuesto, codigoPorcentaje);
    }

    public ParametrosFacturacion Actualizar(
        Ambiente ambiente,
        string tipoEmision,
        bool agenteRetencion,
        string? contribuyenteRimpe,
        string estab,
        string puntoEmision,
        string? contribuyenteEspecial,
        bool obligadoContabilidad,
        string moneda,
        string codigoImpuesto,
        CodigoIva codigoPorcentaje)
    {
        return Aplicar(
            ambiente, tipoEmision, agenteRetencion, contribuyenteRimpe,
            estab, puntoEmision, contribuyenteEspecial, obligadoContabilidad,
            moneda, codigoImpuesto, codigoPorcentaje);
    }

    private ParametrosFacturacion Aplicar(
        Ambiente ambiente,
        string tipoEmision,
        bool agenteRetencion,
        string? contribuyenteRimpe,
        string estab,
        string puntoEmision,
        string? contribuyenteEspecial,
        bool obligadoContabilidad,
        string moneda,
        string codigoImpuesto,
        CodigoIva codigoPorcentaje)
    {
        Ambiente = ambiente;
        TipoEmision = string.IsNullOrWhiteSpace(tipoEmision) ? "1" : tipoEmision.Trim();
        AgenteRetencion = agenteRetencion;
        ContribuyenteRimpe = string.IsNullOrWhiteSpace(contribuyenteRimpe) ? null : contribuyenteRimpe.Trim();
        Estab = estab.Trim().PadLeft(3, '0');
        PuntoEmision = puntoEmision.Trim().PadLeft(3, '0');
        ContribuyenteEspecial = string.IsNullOrWhiteSpace(contribuyenteEspecial) ? null : contribuyenteEspecial.Trim();
        ObligadoContabilidad = obligadoContabilidad;
        Moneda = string.IsNullOrWhiteSpace(moneda) ? "USD" : moneda.Trim();
        CodigoImpuesto = string.IsNullOrWhiteSpace(codigoImpuesto) ? "2" : codigoImpuesto.Trim();
        CodigoPorcentaje = codigoPorcentaje;
        FechaActualizacion = DateTimeOffset.UtcNow;
        UpdatedAt = FechaActualizacion;
        return this;
    }
}
