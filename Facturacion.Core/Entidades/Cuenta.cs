namespace Facturacion.Core.Entidades;

public class Cuenta
{
    protected Cuenta() { }

    public Guid Id { get; private set; }
    public string Plan { get; private set; } = null!;
    public int MaxEmpresas { get; private set; }
    public int MaxUsuarios { get; private set; }
    public DateTimeOffset? FechaExpira { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Cuenta Crear(string plan, int maxEmpresas, int maxUsuarios, DateTimeOffset? fechaExpira = null)
    {
        return new Cuenta
        {
            Id = Guid.NewGuid(),
            Plan = plan,
            MaxEmpresas = maxEmpresas,
            MaxUsuarios = maxUsuarios,
            FechaExpira = fechaExpira,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }
}
