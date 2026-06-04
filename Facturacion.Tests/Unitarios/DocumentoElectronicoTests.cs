using Facturacion.Core.Enums;
using Facturacion.Tests.Soporte;
using FluentAssertions;

namespace Facturacion.Tests.Unitarios;

public class DocumentoElectronicoTests
{
    [Fact]
    public void FacturaNueva_IniciaPendienteYSinArchivos()
    {
        var factura = TestData.FacturaPendiente();

        factura.EstadoSri.Should().Be(EstadoSri.Pendiente);
        factura.EstadoCorreo.Should().Be(EstadoCorreo.Pendiente);
        factura.XmlFirmadoPath.Should().BeNull();
        factura.XmlAutorizadoPath.Should().BeNull();
        factura.PdfPath.Should().BeNull();
    }

    [Fact]
    public void FlujoAutorizado_RegistraEstadosYRutasEnOrden()
    {
        var factura = TestData.FacturaPendiente();
        var fechaAutorizacion = new DateTimeOffset(2026, 6, 4, 12, 0, 0, TimeSpan.Zero);

        factura.RegistrarXmlFirmado("firmado.xml");
        factura.EstadoSri.Should().Be(EstadoSri.Enviado);
        factura.XmlFirmadoPath.Should().Be("firmado.xml");

        factura.RegistrarEnvioSri();
        factura.EstadoSri.Should().Be(EstadoSri.PendienteAutorizacion);

        factura.RegistrarNumeroAutorizacion("AUT-1", fechaAutorizacion, "RECIBIDA");
        factura.NumeroAutorizacion.Should().Be("AUT-1");
        factura.FechaAutorizacion.Should().Be(fechaAutorizacion);
        factura.EstadoSri.Should().Be(EstadoSri.PendienteAutorizacion);

        factura.RegistrarAutorizacionSri("AUT-1", fechaAutorizacion, "autorizado.xml", "AUTORIZADO");
        factura.EstadoSri.Should().Be(EstadoSri.AutorizadoPendienteArchivos);
        factura.XmlFirmadoPath.Should().BeNull();
        factura.XmlAutorizadoPath.Should().Be("autorizado.xml");

        factura.RegistrarPdf("ride.pdf");
        factura.EstadoSri.Should().Be(EstadoSri.Autorizado);
        factura.PdfPath.Should().Be("ride.pdf");
    }

    [Fact]
    public void NoAutorizado_LimpiaXmlFirmadoYGuardaRespuestaSri()
    {
        var factura = TestData.FacturaPendiente();
        factura.RegistrarXmlFirmado("firmado.xml");

        factura.RegistrarNoAutorizacion("ERROR SRI");

        factura.EstadoSri.Should().Be(EstadoSri.NoAutorizado);
        factura.XmlFirmadoPath.Should().BeNull();
        factura.SriRespuesta.Should().Be("ERROR SRI");
    }

    [Fact]
    public void Anular_YMarcarCorreoEnviado_ActualizanEstados()
    {
        var factura = TestData.FacturaPendiente();

        factura.Anular();
        factura.MarcarCorreoEnviado();

        factura.EstadoSri.Should().Be(EstadoSri.Anulado);
        factura.EstadoCorreo.Should().Be(EstadoCorreo.Enviado);
    }
}
