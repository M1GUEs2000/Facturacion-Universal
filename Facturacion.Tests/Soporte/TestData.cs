using Facturacion.Api.Contratos.Facturas;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Metodos;
using FacturaInfoAdicionalRequest = Facturacion.Api.Contratos.Facturas.InfoAdicionalRequest;

namespace Facturacion.Tests.Soporte;

internal static class TestData
{
    internal const string Ruc = "1790012345001";
    internal const string OtroRuc = "1790099999001";
    internal static readonly Guid CuentaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    internal static readonly Guid OtraCuentaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    internal static readonly DateOnly FechaEmision = new(2026, 6, 4);

    internal static Empresa Empresa(Guid? cuentaId = null, string ruc = Ruc, string? logoPath = "1790012345001/logo.webp")
        => Facturacion.Core.Entidades.Empresa.Crear(
            ruc,
            "Empresa Demo",
            "Av. Siempre Viva",
            RutasStorage.Certificado(ruc),
            "cert-secret",
            cuentaId ?? CuentaId,
            "Demo",
            logoPath,
            logoPath is null ? null : "image/webp");

    internal static Cuenta Cuenta()
        => Facturacion.Core.Entidades.Cuenta.Crear("pro", 10, 10);

    internal static Factura FacturaPendiente(string ruc = Ruc)
    {
        var clave = GeneradorClaveAcceso.Generar(
            FechaEmision,
            TipoDocumentoSri.Factura,
            ruc,
            Ambiente.Pruebas,
            "001",
            "002",
            "000000123",
            "12345678");

        return Facturacion.Core.Entidades.Factura.Crear(
            ruc,
            Ambiente.Pruebas,
            "001",
            "002",
            "000000123",
            clave,
            FechaEmision,
            "05",
            "0912345678",
            "Cliente Demo",
            "Direccion cliente",
            "Direccion establecimiento",
            100m,
            0m,
            null,
            null,
            100m,
            12m,
            0m,
            112m,
            null,
            [new FormaPago("01", 112m)],
            [new InfoAdicional("Email", "cliente@example.com")],
            [FacturaDetalle()]);
    }

    internal static Factura FacturaConArchivos()
    {
        var factura = FacturaPendiente();
        factura.RegistrarXmlFirmado("facturas/1790012345001/clave-firmado.xml");
        factura.RegistrarEnvioSri();
        factura.RegistrarNumeroAutorizacion("1234567890", DateTimeOffset.UtcNow, "AUTORIZADO");
        factura.RegistrarAutorizacionSri("1234567890", DateTimeOffset.UtcNow, "facturas/1790012345001/clave.xml", "AUTORIZADO");
        factura.RegistrarPdf("facturas/1790012345001/clave.pdf");
        return factura;
    }

    internal static FacturaDetalle FacturaDetalle()
        => Core.Entidades.FacturaDetalle.Crear(
            1,
            "PROD-001",
            null,
            "Producto de prueba",
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
            12m);

    internal static EmitirFacturaRequest EmitirFacturaRequestValido()
        => new(
            Ruc,
            Ambiente.Pruebas,
            "001",
            "002",
            null,
            FechaEmision,
            "05",
            "0912345678",
            "Cliente Demo",
            "Direccion cliente",
            "Direccion establecimiento",
            100m,
            0m,
            null,
            null,
            100m,
            12m,
            0m,
            112m,
            null,
            [new FormaPagoRequest("01", 112m)],
            [new FacturaInfoAdicionalRequest("Email", "cliente@example.com")],
            [new DetalleFacturaRequest(
                1,
                "PROD-001",
                null,
                "Producto de prueba",
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
}
