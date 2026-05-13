using Facturacion.Core.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// ValueConverters compartidos entre las configuraciones de EF Core.
/// Las lambdas usan static methods porque los switch expressions no son
/// soportados dentro de árboles de expresión (CS8514).
/// </summary>
internal static class ConvertersEfCore
{
    internal static readonly ValueConverter<Ambiente, string> AmbienteConverter = new(
        v => AmbienteToString(v),
        v => StringToAmbiente(v));

    internal static readonly ValueConverter<EstadoSri, string> EstadoSriConverter = new(
        v => EstadoSriToString(v),
        v => StringToEstadoSri(v));

    internal static readonly ValueConverter<EstadoCorreo, string> EstadoCorreoConverter = new(
        v => EstadoCorreoToString(v),
        v => StringToEstadoCorreo(v));

    // ─── Ambiente ─────────────────────────────────────────────────────────────

    private static string AmbienteToString(Ambiente v) => ((int)v).ToString();
    private static Ambiente StringToAmbiente(string v) => (Ambiente)int.Parse(v);

    // ─── EstadoSri ────────────────────────────────────────────────────────────

    private static string EstadoSriToString(EstadoSri v) => v switch
    {
        EstadoSri.Enviado                       => "ENVIADO",
        EstadoSri.PendienteAutorizacion         => "PENDIENTE_AUTORIZACION",
        EstadoSri.AutorizadoPendienteArchivos   => "AUTORIZADO_PENDIENTE_ARCHIVOS",
        EstadoSri.Autorizado                    => "AUTORIZADO",
        EstadoSri.NoAutorizado                  => "NO_AUTORIZADO",
        EstadoSri.Anulado                       => "ANULADO",
        _                                       => "PENDIENTE"
    };

    private static EstadoSri StringToEstadoSri(string v) => v switch
    {
        "ENVIADO"                        => EstadoSri.Enviado,
        "PENDIENTE_AUTORIZACION"         => EstadoSri.PendienteAutorizacion,
        "AUTORIZADO_PENDIENTE_ARCHIVOS"  => EstadoSri.AutorizadoPendienteArchivos,
        "AUTORIZADO"                     => EstadoSri.Autorizado,
        "NO_AUTORIZADO"                  => EstadoSri.NoAutorizado,
        "ANULADO"                        => EstadoSri.Anulado,
        _                                => EstadoSri.Pendiente
    };

    // ─── EstadoCorreo ─────────────────────────────────────────────────────────

    private static string EstadoCorreoToString(EstadoCorreo v) =>
        v == EstadoCorreo.Enviado ? "ENVIADO" : "PENDIENTE";

    private static EstadoCorreo StringToEstadoCorreo(string v) =>
        v == "ENVIADO" ? EstadoCorreo.Enviado : EstadoCorreo.Pendiente;
}
