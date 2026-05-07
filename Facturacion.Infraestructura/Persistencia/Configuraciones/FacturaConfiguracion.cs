using System.Text.Json;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class FacturaConfiguracion : IEntityTypeConfiguration<Factura>
{

    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("facturas");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.EmpresaRuc).HasColumnName("empresa_ruc").HasMaxLength(13).IsRequired();
        builder.Property(f => f.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(f => f.Ambiente).HasColumnName("ambiente").HasMaxLength(1).HasConversion(ConvertersEfCore.AmbienteConverter).IsRequired();
        builder.Property(f => f.Estab).HasColumnName("estab").HasMaxLength(3).IsRequired();
        builder.Property(f => f.PtoEmi).HasColumnName("pto_emi").HasMaxLength(3).IsRequired();
        builder.Property(f => f.Secuencial).HasColumnName("secuencial").HasMaxLength(9).IsRequired();
        builder.Property(f => f.ClaveAcceso).HasColumnName("clave_acceso").HasMaxLength(49).IsRequired();
        builder.Property(f => f.FechaEmision).HasColumnName("fecha_emision").IsRequired();
        builder.Property(f => f.TipoIdentificacionComprador).HasColumnName("tipo_identificacion_comprador").HasMaxLength(2).IsRequired();
        builder.Property(f => f.IdentificacionComprador).HasColumnName("identificacion_comprador").HasMaxLength(20).IsRequired();
        builder.Property(f => f.RazonSocialComprador).HasColumnName("razon_social_comprador").IsRequired();
        builder.Property(f => f.DireccionComprador).HasColumnName("direccion_comprador");
        builder.Property(f => f.DirEstablecimiento).HasColumnName("dir_establecimiento");
        builder.Property(f => f.TotalSinImpuestos).HasColumnName("total_sin_impuestos").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(f => f.TotalDescuento).HasColumnName("total_descuento").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(f => f.BaseImponibleIce).HasColumnName("base_imponible_ice").HasColumnType("numeric(12,2)");
        builder.Property(f => f.ValorIce).HasColumnName("valor_ice").HasColumnType("numeric(12,2)");
        builder.Property(f => f.BaseImponibleIva).HasColumnName("base_imponible_iva").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(f => f.ValorIva).HasColumnName("valor_iva").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(f => f.Propina).HasColumnName("propina").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(f => f.ImporteTotal).HasColumnName("importe_total").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(f => f.GuiaRemision).HasColumnName("guia_remision").HasMaxLength(20);

        builder.Property(f => f.FormasPago)
            .HasColumnName("formas_pago")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<FormaPago>>(v, (JsonSerializerOptions?)null) ?? new());

        builder.Property(f => f.InfoAdicional)
            .HasColumnName("info_adicional")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<InfoAdicional>>(v, (JsonSerializerOptions?)null) ?? new());

        builder.Property(f => f.EstadoSri).HasColumnName("estado_sri").HasMaxLength(30).HasConversion(ConvertersEfCore.EstadoSriConverter).IsRequired();
        builder.Property(f => f.EstadoCorreo).HasColumnName("estado_correo").HasMaxLength(20).HasConversion(ConvertersEfCore.EstadoCorreoConverter).IsRequired();
        builder.Property(f => f.NumeroAutorizacion).HasColumnName("numero_autorizacion").HasMaxLength(49);
        builder.Property(f => f.FechaAutorizacion).HasColumnName("fecha_autorizacion");
        builder.Property(f => f.SriRespuesta).HasColumnName("sri_respuesta").HasColumnType("jsonb");
        builder.Property(f => f.XmlFirmadoPath).HasColumnName("xml_firmado_path");
        builder.Property(f => f.XmlAutorizadoPath).HasColumnName("xml_autorizado_path");
        builder.Property(f => f.PdfPath).HasColumnName("pdf_path");
        builder.Property(f => f.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(f => f.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(f => f.ClaveAcceso).IsUnique();
        builder.HasIndex(f => new { f.EmpresaRuc, f.EstadoSri, f.FechaEmision });

        builder.HasMany(f => f.Detalle)
            .WithOne()
            .HasForeignKey(d => d.FacturaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
