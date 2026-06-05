using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.Facturas;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Tests.Soporte;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Facturacion.Tests.Unitarios;

public class EmitirFacturaTests
{
    private readonly IEmpresasRepositorio _empresas = Substitute.For<IEmpresasRepositorio>();
    private readonly IFacturasRepositorio _facturas = Substitute.For<IFacturasRepositorio>();
    private readonly IParametrosFacturacionRepositorio _parametros = Substitute.For<IParametrosFacturacionRepositorio>();
    private readonly ISecuencialesSriRepositorio _secuenciales = Substitute.For<ISecuencialesSriRepositorio>();
    private readonly IServicioXml _xml = Substitute.For<IServicioXml>();
    private readonly IServicioPdf _pdf = Substitute.For<IServicioPdf>();
    private readonly IServicioStorageFirmaYLogo _storageFirma = Substitute.For<IServicioStorageFirmaYLogo>();
    private readonly IServicioFirma _firma = Substitute.For<IServicioFirma>();
    private readonly IServicioSri _sri = Substitute.For<IServicioSri>();
    private readonly IServicioStorage _storageDocumentos = Substitute.For<IServicioStorage>();

    private EmitirFactura CrearUseCase()
    {
        var orquestador = new OrquestadorEmision(_firma, _sri, _storageDocumentos,
            NullLogger<OrquestadorEmision>.Instance);
        return new EmitirFactura(
            _empresas,
            _facturas,
            _parametros,
            _secuenciales,
            _xml,
            _pdf,
            _storageFirma,
            orquestador);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoEmpresaNoExiste_RetornaNoEncontrada()
    {
        _empresas.ObtenerPorRucAsync(TestData.Ruc, Arg.Any<CancellationToken>())
            .Returns((Empresa?)null);

        var result = await CrearUseCase().EjecutarAsync(ComandoValido());

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Empresa.NoEncontrada);
        await _storageFirma.DidNotReceiveWithAnyArgs().ObtenerAsync(default!);
        await _facturas.DidNotReceiveWithAnyArgs().AgregarAsync(default!);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoEmpresaPerteneceAOtraCuenta_RetornaProhibido()
    {
        _empresas.ObtenerPorRucAsync(TestData.Ruc, Arg.Any<CancellationToken>())
            .Returns(TestData.Empresa(TestData.OtraCuentaId));

        var result = await CrearUseCase().EjecutarAsync(ComandoValido(cuentaId: TestData.CuentaId));

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Empresa.Prohibido);
        await _storageFirma.DidNotReceiveWithAnyArgs().ObtenerAsync(default!);
        await _facturas.DidNotReceiveWithAnyArgs().AgregarAsync(default!);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoSecuencialManualYaExiste_RetornaDuplicadoSinFirmar()
    {
        _empresas.ObtenerPorRucAsync(TestData.Ruc, Arg.Any<CancellationToken>())
            .Returns(TestData.Empresa(TestData.CuentaId));
        _storageFirma.ObtenerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ErrorOr<byte[]>)"cert"u8.ToArray());
        _facturas.ExisteSecuencialActivoAsync(
                TestData.Ruc,
                "001",
                "002",
                "000000123",
                Ambiente.Pruebas,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await CrearUseCase().EjecutarAsync(ComandoValido(secuencial: "000000123"));

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Factura.SecuencialDuplicado);
        await _secuenciales.DidNotReceiveWithAnyArgs().IncrementarYObtenerAsync(default!, default!);
        _xml.DidNotReceiveWithAnyArgs().GenerarXmlFactura(default!, default!);
        await _firma.DidNotReceiveWithAnyArgs().FirmarXmlAsync(default!, default!, default!);
        await _facturas.DidNotReceiveWithAnyArgs().AgregarAsync(default!);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoNoHaySecuencialManual_IncrementaYFormateaANueveDigitos()
    {
        ConfigurarFlujoExitoso();
        _secuenciales.IncrementarYObtenerAsync(TestData.Ruc, "01", Arg.Any<CancellationToken>())
            .Returns((ErrorOr<long>)124);
        Factura? facturaPersistida = null;
        _facturas.AgregarAsync(Arg.Do<Factura>(f => facturaPersistida = f), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await CrearUseCase().EjecutarAsync(ComandoValido(secuencial: null));

        result.IsError.Should().BeFalse();
        facturaPersistida.Should().NotBeNull();
        facturaPersistida!.Secuencial.Should().Be("000000124");
        facturaPersistida.ClaveAcceso.Should().Contain("001002000000124");
        await _secuenciales.Received(1).IncrementarYObtenerAsync(TestData.Ruc, "01", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_CuandoUsaSecuencialManual_NoIncrementaContador()
    {
        ConfigurarFlujoExitoso();
        _facturas.ExisteSecuencialActivoAsync(
                TestData.Ruc,
                "001",
                "002",
                "000000777",
                Ambiente.Pruebas,
                Arg.Any<CancellationToken>())
            .Returns(false);
        Factura? facturaPersistida = null;
        _facturas.AgregarAsync(Arg.Do<Factura>(f => facturaPersistida = f), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await CrearUseCase().EjecutarAsync(ComandoValido(secuencial: "000000777"));

        result.IsError.Should().BeFalse();
        facturaPersistida.Should().NotBeNull();
        facturaPersistida!.Secuencial.Should().Be("000000777");
        await _secuenciales.DidNotReceiveWithAnyArgs().IncrementarYObtenerAsync(default!, default!);
    }

    [Fact]
    public async Task EjecutarAsync_FlujoExitoso_FirmaEnviaAutorizaGuardaXmlYPdf()
    {
        ConfigurarFlujoExitoso();
        _secuenciales.IncrementarYObtenerAsync(TestData.Ruc, "01", Arg.Any<CancellationToken>())
            .Returns((ErrorOr<long>)124);

        var result = await CrearUseCase().EjecutarAsync(ComandoValido(secuencial: null));

        result.IsError.Should().BeFalse();
        result.Value.EstadoSri.Should().Be(EstadoSri.Autorizado);
        result.Value.XmlFirmadoPath.Should().BeNull();
        result.Value.XmlAutorizadoPath.Should().NotBeNull();
        result.Value.PdfPath.Should().NotBeNull();
        _xml.Received(1).GenerarXmlFactura(Arg.Any<Factura>(), Arg.Any<Empresa>(), Arg.Any<ParametrosFacturacion?>());
        await _firma.Received(1).FirmarXmlAsync("<factura/>", Arg.Any<byte[]>(), "cert-secret", Arg.Any<CancellationToken>());
        await _sri.Received(1).EnviarDocumentoAsync("<factura-firmada/>", Ambiente.Pruebas, Arg.Any<CancellationToken>());
        await _sri.Received(1).ConsultarAutorizacionAsync(Arg.Any<string>(), Ambiente.Pruebas, Arg.Any<CancellationToken>());
        await _pdf.Received(1).GenerarRideFacturaAsync(
            Arg.Any<Factura>(),
            Arg.Any<Empresa>(),
            Arg.Any<ParametrosFacturacion?>(),
            Arg.Any<byte[]?>(),
            Arg.Any<CancellationToken>());
        await _facturas.Received().ActualizarAsync(Arg.Is<Factura>(f => f.EstadoSri == EstadoSri.Autorizado), Arg.Any<CancellationToken>());
    }

    private void ConfigurarFlujoExitoso()
    {
        _empresas.ObtenerPorRucAsync(TestData.Ruc, Arg.Any<CancellationToken>())
            .Returns(TestData.Empresa(TestData.CuentaId));
        _storageFirma.ObtenerAsync(Arg.Is<string>(ruta => ruta.EndsWith("certificado.p12")), Arg.Any<CancellationToken>())
            .Returns((ErrorOr<byte[]>)"cert"u8.ToArray());
        _storageFirma.ObtenerAsync(Arg.Is<string>(ruta => ruta.EndsWith("logo.webp")), Arg.Any<CancellationToken>())
            .Returns((ErrorOr<byte[]>)"logo"u8.ToArray());
        _parametros.ObtenerPorEmpresaAsync(TestData.Ruc, Arg.Any<CancellationToken>())
            .Returns((ParametrosFacturacion?)null);
        _xml.GenerarXmlFactura(Arg.Any<Factura>(), Arg.Any<Empresa>(), Arg.Any<ParametrosFacturacion?>())
            .Returns((ErrorOr<string>)"<factura/>");
        _firma.FirmarXmlAsync("<factura/>", Arg.Any<byte[]>(), "cert-secret", Arg.Any<CancellationToken>())
            .Returns((ErrorOr<string>)"<factura-firmada/>");
        _storageDocumentos.GuardarAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => (ErrorOr<string>)call.ArgAt<string>(1));
        _storageDocumentos.EliminarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ErrorOr<bool>)true);
        _sri.EnviarDocumentoAsync("<factura-firmada/>", Ambiente.Pruebas, Arg.Any<CancellationToken>())
            .Returns((ErrorOr<RespuestaRecepcionSri>)new RespuestaRecepcionSri("RECIBIDA", []));
        _sri.ConsultarAutorizacionAsync(Arg.Any<string>(), Ambiente.Pruebas, Arg.Any<CancellationToken>())
            .Returns((ErrorOr<RespuestaAutorizacionSri>)new RespuestaAutorizacionSri(
                true,
                "AUT-123",
                DateTimeOffset.UtcNow,
                "<autorizado/>",
                "AUTORIZADO",
                []));
        _pdf.GenerarRideFacturaAsync(
                Arg.Any<Factura>(),
                Arg.Any<Empresa>(),
                Arg.Any<ParametrosFacturacion?>(),
                Arg.Any<byte[]?>(),
                Arg.Any<CancellationToken>())
            .Returns((ErrorOr<byte[]>)"pdf"u8.ToArray());
    }

    private static ComandoEmitirFactura ComandoValido(string? secuencial = null, Guid? cuentaId = null)
        => new(
            TestData.Ruc,
            Ambiente.Pruebas,
            "001",
            "002",
            secuencial,
            TestData.FechaEmision,
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
            [new ComandoDetalleFactura(
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
                12m)],
            "127.0.0.1",
            cuentaId ?? TestData.CuentaId);
}
