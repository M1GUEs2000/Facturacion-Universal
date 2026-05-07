using Facturacion.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class RetencionDetalleConfiguracion : IEntityTypeConfiguration<RetencionDetalle>
{
    public void Configure(EntityTypeBuilder<RetencionDetalle> builder)
    {
        builder.ToTable("retenciones_detalle");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.RetencionId).HasColumnName("retencion_id").IsRequired();
        builder.Property(d => d.Orden).HasColumnName("orden").IsRequired();
        builder.Property(d => d.CodigoImpuesto).HasColumnName("codigo_impuesto").HasMaxLength(1).IsRequired();
        builder.Property(d => d.CodigoRetencion).HasColumnName("codigo_retencion").HasMaxLength(5).IsRequired();
        builder.Property(d => d.BaseImponible).HasColumnName("base_imponible").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(d => d.PorcentajeRetener).HasColumnName("porcentaje_retener").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(d => d.ValorRetenido).HasColumnName("valor_retenido").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(d => d.CodDocSustento).HasColumnName("cod_doc_sustento").HasMaxLength(2).IsRequired();
        builder.Property(d => d.NumDocSustento).HasColumnName("num_doc_sustento").HasMaxLength(17).IsRequired();
        builder.Property(d => d.FechaEmisionDocSustento).HasColumnName("fecha_emision_doc_sustento").IsRequired();

        builder.HasIndex(d => d.RetencionId);
    }
}
