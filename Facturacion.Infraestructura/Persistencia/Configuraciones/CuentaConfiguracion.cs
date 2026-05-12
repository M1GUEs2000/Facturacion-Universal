using Facturacion.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class CuentaConfiguracion : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("cuentas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Plan).HasColumnName("plan").IsRequired();
        builder.Property(c => c.MaxEmpresas).HasColumnName("max_empresas").IsRequired();
        builder.Property(c => c.MaxUsuarios).HasColumnName("max_usuarios").IsRequired();
        builder.Property(c => c.FechaExpira).HasColumnName("fecha_expira");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
