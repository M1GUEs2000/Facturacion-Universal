using Facturacion.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class ParametrosFacturacionConfiguracion : IEntityTypeConfiguration<ParametrosFacturacion>
{
    public void Configure(EntityTypeBuilder<ParametrosFacturacion> builder)
    {
        builder.ToTable("parametros_facturacion");

        builder.HasKey(p => p.EmpresaRuc);

        builder.Property(p => p.EmpresaRuc).HasColumnName("empresa_ruc").HasMaxLength(13).IsRequired();
        builder.Property(p => p.Ambiente).HasColumnName("ambiente").HasMaxLength(1).HasConversion(ConvertersEfCore.AmbienteConverter).IsRequired();
        builder.Property(p => p.TipoEmision).HasColumnName("tipo_emision").HasMaxLength(1).IsRequired();
        builder.Property(p => p.AgenteRetencion).HasColumnName("agente_retencion").IsRequired();
        builder.Property(p => p.ContribuyenteRimpe).HasColumnName("contribuyente_rimpe").HasMaxLength(100);
        builder.Property(p => p.Estab).HasColumnName("estab").HasMaxLength(3).IsRequired();
        builder.Property(p => p.PuntoEmision).HasColumnName("punto_emision").HasMaxLength(3).IsRequired();
        builder.Property(p => p.ContribuyenteEspecial).HasColumnName("contribuyente_especial").HasMaxLength(13);
        builder.Property(p => p.ObligadoContabilidad).HasColumnName("obligado_contabilidad").IsRequired();
        builder.Property(p => p.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();
        builder.Property(p => p.CodigoImpuesto).HasColumnName("codigo_impuesto").HasMaxLength(1).IsRequired();
        builder.Property(p => p.CodigoPorcentaje).HasColumnName("codigo_porcentaje").HasConversion<int>().IsRequired();
        builder.Property(p => p.FechaActualizacion).HasColumnName("fecha_actualizacion").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasOne<Empresa>()
            .WithOne()
            .HasForeignKey<ParametrosFacturacion>(p => p.EmpresaRuc)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
