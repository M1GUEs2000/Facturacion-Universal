using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Tests.Soporte;
using FluentAssertions;
using NSubstitute;

namespace Facturacion.Tests.Unitarios;

public class ObtenerUrlDescargaTests
{
    private readonly IEmpresasRepositorio _empresas = Substitute.For<IEmpresasRepositorio>();
    private readonly IServicioStorage _storage = Substitute.For<IServicioStorage>();

    private ObtenerUrlDescarga CrearUseCase() => new(_empresas, _storage);

    [Fact]
    public async Task EjecutarAsync_CuandoEmpresaNoExiste_RetornaNoEncontrada()
    {
        var factura = TestData.FacturaConArchivos();
        _empresas.ObtenerPorRucAsync(factura.EmpresaRuc, Arg.Any<CancellationToken>())
            .Returns((Core.Entidades.Empresa?)null);

        var result = await CrearUseCase().EjecutarAsync(factura, TipoArchivoDescarga.Pdf, TestData.CuentaId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Empresa.NoEncontrada);
        await _storage.DidNotReceiveWithAnyArgs().GenerarUrlFirmadaAsync(default!);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoEmpresaEsDeOtraCuenta_RetornaProhibido()
    {
        var factura = TestData.FacturaConArchivos();
        _empresas.ObtenerPorRucAsync(factura.EmpresaRuc, Arg.Any<CancellationToken>())
            .Returns(TestData.Empresa(TestData.OtraCuentaId));

        var result = await CrearUseCase().EjecutarAsync(factura, TipoArchivoDescarga.Pdf, TestData.CuentaId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Empresa.Prohibido);
        await _storage.DidNotReceiveWithAnyArgs().GenerarUrlFirmadaAsync(default!);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoDocumentoNoTienePdf_RetornaSinPdf()
    {
        var factura = TestData.FacturaPendiente();
        _empresas.ObtenerPorRucAsync(factura.EmpresaRuc, Arg.Any<CancellationToken>())
            .Returns(TestData.Empresa(TestData.CuentaId));

        var result = await CrearUseCase().EjecutarAsync(factura, TipoArchivoDescarga.Pdf, TestData.CuentaId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Documento.SinPdf);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoDocumentoNoTieneXml_RetornaSinXml()
    {
        var factura = TestData.FacturaPendiente();
        _empresas.ObtenerPorRucAsync(factura.EmpresaRuc, Arg.Any<CancellationToken>())
            .Returns(TestData.Empresa(TestData.CuentaId));

        var result = await CrearUseCase().EjecutarAsync(factura, TipoArchivoDescarga.Xml, TestData.CuentaId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Documento.SinXml);
    }

    [Theory]
    [InlineData(TipoArchivoDescarga.Pdf, "facturas/1790012345001/clave.pdf")]
    [InlineData(TipoArchivoDescarga.Xml, "facturas/1790012345001/clave.xml")]
    public async Task EjecutarAsync_GeneraUrlFirmadaConTtlDeCincoMinutos(TipoArchivoDescarga tipo, string rutaEsperada)
    {
        var factura = TestData.FacturaConArchivos();
        _empresas.ObtenerPorRucAsync(factura.EmpresaRuc, Arg.Any<CancellationToken>())
            .Returns(TestData.Empresa(TestData.CuentaId));
        _storage.GenerarUrlFirmadaAsync(rutaEsperada, 300, Arg.Any<CancellationToken>())
            .Returns((ErrorOr<string>)"https://storage.local/signed");
        var antes = DateTimeOffset.UtcNow;

        var result = await CrearUseCase().EjecutarAsync(factura, tipo, TestData.CuentaId);

        result.IsError.Should().BeFalse();
        result.Value.Url.Should().Be("https://storage.local/signed");
        result.Value.ExpiresAt.Should().BeOnOrAfter(antes.AddSeconds(299));
        result.Value.ExpiresAt.Should().BeOnOrBefore(DateTimeOffset.UtcNow.AddSeconds(301));
        await _storage.Received(1).GenerarUrlFirmadaAsync(rutaEsperada, 300, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_PropagaErrorDeStorage()
    {
        var factura = TestData.FacturaConArchivos();
        _empresas.ObtenerPorRucAsync(factura.EmpresaRuc, Arg.Any<CancellationToken>())
            .Returns(TestData.Empresa(TestData.CuentaId));
        _storage.GenerarUrlFirmadaAsync(factura.PdfPath!, 300, Arg.Any<CancellationToken>())
            .Returns(Errores.Storage.ErrorGuardar);

        var result = await CrearUseCase().EjecutarAsync(factura, TipoArchivoDescarga.Pdf, TestData.CuentaId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Storage.ErrorGuardar);
    }
}
