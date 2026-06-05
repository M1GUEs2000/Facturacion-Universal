using Serilog.Core;
using Serilog.Events;

namespace Facturacion.Api.Extensions;

/// <summary>
/// Redacta propiedades sensibles (CertPassword, CertificadoP12Base64) cuando algún
/// objeto es destruido con {@obj} en un log, evitando que acaben en consola o sinks.
/// </summary>
public sealed class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    private static readonly HashSet<string> Sensitive = new(StringComparer.OrdinalIgnoreCase)
    {
        "CertPassword",
        "CertificadoP12Base64",
    };

    public bool TryDestructure(
        object value,
        ILogEventPropertyValueFactory propertyValueFactory,
        out LogEventPropertyValue result)
    {
        var props = value.GetType().GetProperties();
        if (!props.Any(p => Sensitive.Contains(p.Name)))
        {
            result = null!;
            return false;
        }

        var logProps = props.Select(p => new LogEventProperty(
            p.Name,
            Sensitive.Contains(p.Name)
                ? new ScalarValue("***REDACTED***")
                : propertyValueFactory.CreatePropertyValue(p.GetValue(value), true)));

        result = new StructureValue(logProps);
        return true;
    }
}
