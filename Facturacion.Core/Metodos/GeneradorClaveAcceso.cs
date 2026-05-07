using Facturacion.Core.Enums;

namespace Facturacion.Core.Metodos;

public static class GeneradorClaveAcceso
{
    // Pesos del módulo 11 SRI (ciclan de derecha a izquierda: 2,3,4,5,6,7)
    private static readonly int[] Pesos = [2, 3, 4, 5, 6, 7];

    public static string Generar(
        DateOnly fechaEmision,
        string tipoDocumento,
        string ruc,
        Ambiente ambiente,
        string estab,
        string ptoEmi,
        string secuencial,
        string? codigoNumerico = null)
    {
        codigoNumerico ??= Random.Shared.Next(0, 99_999_999).ToString("D8");

        // 8 + 2 + 13 + 1 + 3 + 3 + 9 + 8 + 1 = 48 dígitos base
        var base48 = string.Concat(
            fechaEmision.ToString("ddMMyyyy"),
            tipoDocumento,
            ruc,
            ((int)ambiente).ToString(),
            estab.PadLeft(3, '0'),
            ptoEmi.PadLeft(3, '0'),
            secuencial.PadLeft(9, '0'),
            codigoNumerico,
            "1"  // tipo_emision = normal
        );

        return base48 + CalcularDigitoVerificador(base48);
    }

    private static int CalcularDigitoVerificador(string base48)
    {
        int suma = 0;
        for (int i = base48.Length - 1; i >= 0; i--)
        {
            int peso = Pesos[(base48.Length - 1 - i) % Pesos.Length];
            suma += (base48[i] - '0') * peso;
        }

        int resultado = 11 - (suma % 11);
        return resultado switch
        {
            11 => 0,
            10 => 1,
            _ => resultado
        };
    }
}
