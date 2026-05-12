using Facturacion.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class SecuencialSriConfiguracion : IEntityTypeConfiguration<SecuencialSri>
{
    public void Configure(EntityTypeBuilder<SecuencialSri> builder)
    {
        builder.ToTable("secuenciales_sri");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id").IsRequired();
        builder.Property(p => p.EmpresaRuc).HasColumnName("empresa_ruc").HasMaxLength(13).IsRequired();
        builder.Property(p => p.TipoComprobante).HasColumnName("tipo_comprobante").HasMaxLength(2).IsRequired();
        builder.Property(p => p.Secuencial).HasColumnName("secuencial").IsRequired();
        builder.Property(p => p.CodigoNumerico).HasColumnName("codigo_numerico").HasMaxLength(8).IsRequired();
        builder.Property(p => p.FechaActualizacion).HasColumnName("fecha_actualizacion").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(p => new { p.EmpresaRuc, p.TipoComprobante }).IsUnique();
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(p => p.EmpresaRuc)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
