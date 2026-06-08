using System.Text;

namespace Facturacion.Core.Interfaces.Comun;

public sealed record CursorDePagina(DateTimeOffset CreatedAt, Guid Id)
{
    // Encodes as base64url of "{utc_ticks}:{guid}"
    public string Encode()
    {
        var raw = $"{CreatedAt.UtcTicks}:{Id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static CursorDePagina? Decode(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            var padded = token.Replace('-', '+').Replace('_', '/')
                + new string('=', (4 - token.Length % 4) % 4);
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            var sep = raw.IndexOf(':');
            if (sep < 1) return null;
            if (!long.TryParse(raw[..sep], out var ticks)) return null;
            if (!Guid.TryParse(raw[(sep + 1)..], out var id)) return null;
            return new CursorDePagina(new DateTimeOffset(ticks, TimeSpan.Zero), id);
        }
        catch { return null; }
    }
}
