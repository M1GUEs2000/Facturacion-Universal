using ErrorOr;
using Facturacion.Core;
using Facturacion.Core.CasosDeUso.Cuentas;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Core.Metodos;
using Facturacion.Tests.Soporte;
using FluentAssertions;
using NSubstitute;

namespace Facturacion.Tests.Unitarios;

public class EliminarCuentaTests
{
    private readonly ICuentasRepositorio _cuentas = Substitute.For<ICuentasRepositorio>();
    private readonly IEmpresasRepositorio _empresas = Substitute.For<IEmpresasRepositorio>();
    private readonly IServicioStorage _storageDocumentos = Substitute.For<IServicioStorage>();
    private readonly IServicioStorageFirmaYLogo _storageFirmaYLogo = Substitute.For<IServicioStorageFirmaYLogo>();

    private EliminarCuenta CrearUseCase()
        => new(_cuentas, _empresas, _storageDocumentos, _storageFirmaYLogo);

    [Fact]
    public async Task EjecutarAsync_CuandoCuentaNoCoincideConJwt_RetornaProhibidoSinTocarRepositorios()
    {
        var useCase = CrearUseCase();

        var result = await useCase.EjecutarAsync(TestData.CuentaId, TestData.OtraCuentaId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Cuenta.Prohibido);
        await _cuentas.DidNotReceiveWithAnyArgs().ObtenerPorIdAsync(default);
        await _cuentas.DidNotReceiveWithAnyArgs().EliminarCuentaAsync(default, default!);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoCuentaNoExiste_RetornaNoEncontrada()
    {
        _cuentas.ObtenerPorIdAsync(TestData.CuentaId, Arg.Any<CancellationToken>())
            .Returns((Cuenta?)null);
        var useCase = CrearUseCase();

        var result = await useCase.EjecutarAsync(TestData.CuentaId, TestData.CuentaId);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errores.Cuenta.NoEncontrada);
        await _empresas.DidNotReceiveWithAnyArgs().ListarPorCuentaAsync(default);
        await _cuentas.DidNotReceiveWithAnyArgs().EliminarCuentaAsync(default, default!);
    }

    [Fact]
    public async Task EjecutarAsync_CuentaSinEmpresas_EliminaCuentaSinConsultarStorage()
    {
        _cuentas.ObtenerPorIdAsync(TestData.CuentaId, Arg.Any<CancellationToken>())
            .Returns(TestData.Cuenta());
        _empresas.ListarPorCuentaAsync(TestData.CuentaId, 1, 1000, Arg.Any<CancellationToken>())
            .Returns([]);
        var useCase = CrearUseCase();

        var result = await useCase.EjecutarAsync(TestData.CuentaId, TestData.CuentaId);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _cuentas.Received(1).EliminarCuentaAsync(
            TestData.CuentaId,
            Arg.Is<IReadOnlyList<string>>(rucs => rucs.Count == 0),
            Arg.Any<CancellationToken>());
        await _storageDocumentos.DidNotReceiveWithAnyArgs().EliminarAsync(default!);
        await _storageFirmaYLogo.DidNotReceiveWithAnyArgs().EliminarAsync(default!);
    }

    [Fact]
    public async Task EjecutarAsync_EliminaDocumentosCertificadosYLogosAntesDePurgarBd()
    {
        var empresaConLogo = TestData.Empresa(TestData.CuentaId, TestData.Ruc, "1790012345001/logo.webp");
        var empresaSinLogo = TestData.Empresa(TestData.CuentaId, TestData.OtroRuc, null);
        var paths = new List<(string? XmlFirmado, string? XmlAutorizado, string? Pdf)>
        {
            ("docs/f1-firmado.xml", "docs/f1.xml", "docs/f1.pdf"),
            (null, "docs/f2.xml", null)
        };

        _cuentas.ObtenerPorIdAsync(TestData.CuentaId, Arg.Any<CancellationToken>())
            .Returns(TestData.Cuenta());
        _empresas.ListarPorCuentaAsync(TestData.CuentaId, 1, 1000, Arg.Any<CancellationToken>())
            .Returns([empresaConLogo, empresaSinLogo]);
        _cuentas.ObtenerPathsDocumentosPorRucsAsync(
                Arg.Is<IReadOnlyList<string>>(rucs => rucs.SequenceEqual(new[] { TestData.Ruc, TestData.OtroRuc })),
                Arg.Any<CancellationToken>())
            .Returns(paths);
        _storageDocumentos.EliminarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ErrorOr<bool>)true);
        _storageFirmaYLogo.EliminarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ErrorOr<bool>)true);
        var useCase = CrearUseCase();

        var result = await useCase.EjecutarAsync(TestData.CuentaId, TestData.CuentaId);

        result.IsError.Should().BeFalse();
        await _storageDocumentos.Received(1).EliminarAsync("docs/f1-firmado.xml", Arg.Any<CancellationToken>());
        await _storageDocumentos.Received(1).EliminarAsync("docs/f1.xml", Arg.Any<CancellationToken>());
        await _storageDocumentos.Received(1).EliminarAsync("docs/f1.pdf", Arg.Any<CancellationToken>());
        await _storageDocumentos.Received(1).EliminarAsync("docs/f2.xml", Arg.Any<CancellationToken>());
        await _storageDocumentos.Received(4).EliminarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

        await _storageFirmaYLogo.Received(1).EliminarAsync(RutasStorage.Certificado(TestData.Ruc), Arg.Any<CancellationToken>());
        await _storageFirmaYLogo.Received(1).EliminarAsync("1790012345001/logo.webp", Arg.Any<CancellationToken>());
        await _storageFirmaYLogo.Received(1).EliminarAsync(RutasStorage.Certificado(TestData.OtroRuc), Arg.Any<CancellationToken>());
        await _storageFirmaYLogo.Received(3).EliminarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

        await _cuentas.Received(1).EliminarCuentaAsync(
            TestData.CuentaId,
            Arg.Is<IReadOnlyList<string>>(rucs => rucs.SequenceEqual(new[] { TestData.Ruc, TestData.OtroRuc })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EjecutarAsync_StorageBestEffort_NoBloqueaEliminacionBd()
    {
        _cuentas.ObtenerPorIdAsync(TestData.CuentaId, Arg.Any<CancellationToken>())
            .Returns(TestData.Cuenta());
        _empresas.ListarPorCuentaAsync(TestData.CuentaId, 1, 1000, Arg.Any<CancellationToken>())
            .Returns([TestData.Empresa(TestData.CuentaId)]);
        _cuentas.ObtenerPathsDocumentosPorRucsAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns([("docs/f1-firmado.xml", null, null)]);
        _storageDocumentos.EliminarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Errores.Storage.ArchivoNoEncontrado);
        _storageFirmaYLogo.EliminarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Errores.Storage.ArchivoNoEncontrado);
        var useCase = CrearUseCase();

        var result = await useCase.EjecutarAsync(TestData.CuentaId, TestData.CuentaId);

        result.IsError.Should().BeFalse();
        await _cuentas.Received(1).EliminarCuentaAsync(
            TestData.CuentaId,
            Arg.Is<IReadOnlyList<string>>(rucs => rucs.SequenceEqual(new[] { TestData.Ruc })),
            Arg.Any<CancellationToken>());
    }
}
