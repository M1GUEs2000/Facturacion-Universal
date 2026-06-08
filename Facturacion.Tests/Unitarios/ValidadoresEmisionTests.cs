using Facturacion.Api.Contratos.Facturas;
using Facturacion.Api.Contratos.NotasCredito;
using Facturacion.Api.Contratos.Retenciones;
using Facturacion.Core.Enums;
using Facturacion.Tests.Soporte;
using FluentAssertions;
using FacturaDetalleRequest = Facturacion.Api.Contratos.Facturas.DetalleFacturaRequest;
using FacturaInfoAdicionalRequest = Facturacion.Api.Contratos.Facturas.InfoAdicionalRequest;
using NotaCreditoDetalleRequest = Facturacion.Api.Contratos.NotasCredito.DetalleNotaCreditoRequest;
using NotaCreditoInfoAdicionalRequest = Facturacion.Api.Contratos.NotasCredito.InfoAdicionalRequest;
using RetencionInfoAdicionalRequest = Facturacion.Api.Contratos.Retenciones.InfoAdicionalRequest;

namespace Facturacion.Tests.Unitarios;

public class ValidadoresEmisionTests
{
    [Fact]
    public void EmitirFacturaValidator_AceptaRequestValido()
    {
        var validator = new EmitirFacturaValidator();

        var result = validator.Validate(TestData.EmitirFacturaRequestValido());

        result.IsValid.Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("", "001", "002", TipoIdentificacion.Cedula, "0912345678", 112)]
    [InlineData("1790012345001", "01", "002", TipoIdentificacion.Cedula, "0912345678", 112)]
    [InlineData("1790012345001", "001", "2A2", TipoIdentificacion.Cedula, "0912345678", 112)]
    [InlineData("1790012345001", "001", "002", "XX", "0912345678", 112)]
    [InlineData("1790012345001", "001", "002", TipoIdentificacion.Cedula, "", 112)]
    [InlineData("1790012345001", "001", "002", TipoIdentificacion.Cedula, "0912345678", 0)]
    public void EmitirFacturaValidator_RechazaCamposCriticosInvalidos(
        string ruc,
        string estab,
        string ptoEmi,
        string tipoIdentificacion,
        string identificacion,
        decimal importeTotal)
    {
        var req = TestData.EmitirFacturaRequestValido() with
        {
            EmpresaRuc = ruc,
            Estab = estab,
            PtoEmi = ptoEmi,
            TipoIdentificacionComprador = tipoIdentificacion,
            IdentificacionComprador = identificacion,
            ImporteTotal = importeTotal
        };

        var result = new EmitirFacturaValidator().Validate(req);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmitirFacturaValidator_RechazaSecuencialConLongitudInvalida()
    {
        var req = TestData.EmitirFacturaRequestValido() with { Secuencial = "123" };

        var result = new EmitirFacturaValidator().Validate(req);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(EmitirFacturaRequest.Secuencial));
    }

    [Fact]
    public void EmitirFacturaValidator_RechazaDetalleVacioYFormaPagoVacia()
    {
        var req = TestData.EmitirFacturaRequestValido() with
        {
            FormasPago = [],
            Detalle = []
        };

        var result = new EmitirFacturaValidator().Validate(req);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(nameof(EmitirFacturaRequest.FormasPago));
        result.Errors.Select(e => e.PropertyName).Should().Contain(nameof(EmitirFacturaRequest.Detalle));
    }

    [Fact]
    public void EmitirFacturaValidator_RechazaDetalleConCantidadCero()
    {
        var req = TestData.EmitirFacturaRequestValido() with
        {
            Detalle =
            [
                new FacturaDetalleRequest(
                    1,
                    "PROD-001",
                    null,
                    "Producto",
                    0m,
                    100m,
                    0m,
                    100m,
                    null,
                    null,
                    null,
                    null,
                    CodigoIva.Doce,
                    12m,
                    100m,
                    12m)
            ]
        };

        var result = new EmitirFacturaValidator().Validate(req);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.EndsWith(".Cantidad"));
    }

    [Fact]
    public void EmitirNotaCreditoValidator_AceptaRequestValido()
    {
        var req = NotaCreditoValida();

        var result = new EmitirNotaCreditoValidator().Validate(req);

        result.IsValid.Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("", "001", "002", TipoIdentificacion.Cedula, "0406202601179001234500110010020000001231234567812", "Correccion")]
    [InlineData("1790012345001", "001", "002", "XX", "0406202601179001234500110010020000001231234567812", "Correccion")]
    [InlineData("1790012345001", "001", "002", TipoIdentificacion.Cedula, "123", "Correccion")]
    [InlineData("1790012345001", "001", "002", TipoIdentificacion.Cedula, "0406202601179001234500110010020000001231234567812", "")]
    public void EmitirNotaCreditoValidator_RechazaCamposCriticosInvalidos(
        string ruc,
        string estab,
        string ptoEmi,
        string tipoIdentificacion,
        string claveModificada,
        string motivo)
    {
        var req = NotaCreditoValida() with
        {
            EmpresaRuc = ruc,
            Estab = estab,
            PtoEmi = ptoEmi,
            TipoIdentificacionComprador = tipoIdentificacion,
            DocModificadoClaveAcceso = claveModificada,
            Motivo = motivo
        };

        var result = new EmitirNotaCreditoValidator().Validate(req);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmitirRetencionValidator_AceptaRequestValido()
    {
        var req = RetencionValida();

        var result = new EmitirRetencionValidator().Validate(req);

        result.IsValid.Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("2026-06", "1", "303", 100, 10)]
    [InlineData("06/2026", "9", "303", 100, 10)]
    [InlineData("06/2026", "1", "999999", 100, 10)]
    [InlineData("06/2026", "1", "303", 0, 10)]
    [InlineData("06/2026", "1", "303", 100, -1)]
    public void EmitirRetencionValidator_RechazaDetalleFiscalInvalido(
        string periodoFiscal,
        string codigoImpuesto,
        string codigoRetencion,
        decimal baseImponible,
        decimal valorRetenido)
    {
        var req = RetencionValida() with
        {
            PeriodoFiscal = periodoFiscal,
            Detalle =
            [
                new DetalleRetencionRequest(
                    1,
                    codigoImpuesto,
                    codigoRetencion,
                    baseImponible,
                    10m,
                    valorRetenido,
                    TipoDocumentoSri.Factura,
                    "001-002-000000123",
                    TestData.FechaEmision)
            ]
        };

        var result = new EmitirRetencionValidator().Validate(req);

        result.IsValid.Should().BeFalse();
    }

    private static EmitirNotaCreditoRequest NotaCreditoValida()
        => new(
            TestData.Ruc,
            Ambiente.Pruebas,
            "001",
            "002",
            null,
            TestData.FechaEmision,
            TipoIdentificacion.Cedula,
            "0912345678",
            "Cliente Demo",
            "Direccion cliente",
            "Direccion establecimiento",
            TipoDocumentoSri.Factura,
            "001-002-000000123",
            TestData.FechaEmision.AddDays(-1),
            "0406202601179001234500110010020000001231234567812",
            "Correccion",
            100m,
            0m,
            null,
            null,
            100m,
            12m,
            112m,
            [new NotaCreditoInfoAdicionalRequest("Email", "cliente@example.com")],
            [new NotaCreditoDetalleRequest(
                1,
                "PROD-001",
                null,
                "Producto",
                1m,
                100m,
                0m,
                100m,
                null,
                null,
                null,
                null,
                CodigoIva.Doce,
                12m,
                100m,
                12m)]);

    private static EmitirRetencionRequest RetencionValida()
        => new(
            TestData.Ruc,
            Ambiente.Pruebas,
            "001",
            "002",
            null,
            TestData.FechaEmision,
            TipoIdentificacion.Ruc,
            "1790012345001",
            "Proveedor Demo",
            "Direccion proveedor",
            "06/2026",
            100m,
            10m,
            0m,
            10m,
            [new RetencionInfoAdicionalRequest("Email", "proveedor@example.com")],
            [new DetalleRetencionRequest(
                1,
                CodigoImpuestoRetencion.Renta,
                "303",
                100m,
                10m,
                10m,
                TipoDocumentoSri.Factura,
                "001-002-000000123",
                TestData.FechaEmision)]);
}
