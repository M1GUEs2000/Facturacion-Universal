using Facturacion.Core.Enums;
using Facturacion.Core.Metodos;
using FluentAssertions;

namespace Facturacion.Tests.Unitarios;

public class GeneradorClaveAccesoTests
{
    [Fact]
    public void Generar_ConCodigoNumericoDeterministico_ConstruyeClaveDe49Digitos()
    {
        var clave = GeneradorClaveAcceso.Generar(
            new DateOnly(2026, 6, 4),
            TipoDocumentoSri.Factura,
            "1790012345001",
            Ambiente.Pruebas,
            "001",
            "002",
            "000000123",
            "12345678");

        clave.Should().Be("0406202601179001234500110010020000001231234567815");
        clave.Should().HaveLength(49);
    }

    [Theory]
    [InlineData("01", Ambiente.Pruebas, "0406202601179001234500110010020000001231234567815")]
    [InlineData("04", Ambiente.Pruebas, "0406202604179001234500110010020000001231234567814")]
    [InlineData("07", Ambiente.Produccion, "0406202607179001234500120010020000001231234567811")]
    public void Generar_CalculaDigitoVerificadorModulo11(string tipoDocumento, Ambiente ambiente, string esperado)
    {
        var clave = GeneradorClaveAcceso.Generar(
            new DateOnly(2026, 6, 4),
            tipoDocumento,
            "1790012345001",
            ambiente,
            "001",
            "002",
            "000000123",
            "12345678");

        clave.Should().Be(esperado);
        clave[^1].Should().Be(CalcularDigitoEsperado(clave[..48]));
    }

    [Fact]
    public void Generar_RellenaEstablecimientoPuntoYSecuencialConCeros()
    {
        var clave = GeneradorClaveAcceso.Generar(
            new DateOnly(2026, 6, 4),
            TipoDocumentoSri.Factura,
            "1790012345001",
            Ambiente.Pruebas,
            "1",
            "2",
            "123",
            "12345678");

        clave.Substring(24, 3).Should().Be("001");
        clave.Substring(27, 3).Should().Be("002");
        clave.Substring(30, 9).Should().Be("000000123");
    }

    [Fact]
    public void Generar_SinCodigoNumerico_GeneraClaveValidaDe49Digitos()
    {
        var clave = GeneradorClaveAcceso.Generar(
            new DateOnly(2026, 6, 4),
            TipoDocumentoSri.Factura,
            "1790012345001",
            Ambiente.Pruebas,
            "001",
            "002",
            "000000123");

        clave.Should().MatchRegex("^\\d{49}$");
        clave[^1].Should().Be(CalcularDigitoEsperado(clave[..48]));
    }

    private static char CalcularDigitoEsperado(string base48)
    {
        int[] pesos = [2, 3, 4, 5, 6, 7];
        var suma = 0;

        for (var i = base48.Length - 1; i >= 0; i--)
        {
            var posicionDesdeDerecha = base48.Length - 1 - i;
            suma += (base48[i] - '0') * pesos[posicionDesdeDerecha % pesos.Length];
        }

        var resultado = 11 - (suma % 11);
        var digito = resultado switch
        {
            11 => 0,
            10 => 1,
            _ => resultado
        };

        return (char)('0' + digito);
    }
}
