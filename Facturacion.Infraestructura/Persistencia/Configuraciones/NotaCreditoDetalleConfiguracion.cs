using Facturacion.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class NotaCreditoDetalleConfiguracion : IEntityTypeConfiguration<NotaCreditoDetalle>
{
    public void Configure(EntityTypeBuilder<NotaCreditoDetalle> builder)
    {
        builder.ToTable("notas_credito_detalle");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.NotaCreditoId).HasColumnName("nota_credito_id").IsRequired();
        builder.Property(d => d.Orden).HasColumnName("orden").IsRequired();
        builder.Property(d => d.CodigoPrincipal).HasColumnName("codigo_principal").HasMaxLength(25).IsRequired();
        builder.Property(d => d.CodigoAuxiliar).HasColumnName("codigo_auxiliar").HasMaxLength(25);
        builder.Property(d => d.Descripcion).HasColumnName("descripcion").IsRequired();
        builder.Property(d => d.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(12,6)").IsRequired();
        builder.Property(d => d.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(12,6)").IsRequired();
        builder.Property(d => d.Descuento).HasColumnName("descuento").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(d => d.PrecioTotalSinImpuesto).HasColumnName("precio_total_sin_impuesto").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(d => d.IceCodigo).HasColumnName("ice_codigo").HasMaxLength(10);
        builder.Property(d => d.IceTarifa).HasColumnName("ice_tarifa").HasColumnType("numeric(5,2)");
        builder.Property(d => d.IceBase).HasColumnName("ice_base").HasColumnType("numeric(12,2)");
        builder.Property(d => d.IceValor).HasColumnName("ice_valor").HasColumnType("numeric(12,2)");
        builder.Property(d => d.IvaCodigo).HasColumnName("iva_codigo").HasConversion<int>().IsRequired();
        builder.Property(d => d.IvaTarifa).HasColumnName("iva_tarifa").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(d => d.IvaBase).HasColumnName("iva_base").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(d => d.IvaValor).HasColumnName("iva_valor").HasColumnType("numeric(12,2)").IsRequired();

        builder.HasIndex(d => d.NotaCreditoId);
    }
}
