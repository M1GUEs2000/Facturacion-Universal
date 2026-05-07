using System.Text.Json;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class RetencionConfiguracion : IEntityTypeConfiguration<Retencion>
{

    public void Configure(EntityTypeBuilder<Retencion> builder)
    {
        builder.ToTable("retenciones");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.EmpresaRuc).HasColumnName("empresa_ruc").HasMaxLength(13).IsRequired();
        builder.Property(r => r.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(r => r.Ambiente).HasColumnName("ambiente").HasMaxLength(1).HasConversion(ConvertersEfCore.AmbienteConverter).IsRequired();
        builder.Property(r => r.Estab).HasColumnName("estab").HasMaxLength(3).IsRequired();
        builder.Property(r => r.PtoEmi).HasColumnName("pto_emi").HasMaxLength(3).IsRequired();
        builder.Property(r => r.Secuencial).HasColumnName("secuencial").HasMaxLength(9).IsRequired();
        builder.Property(r => r.ClaveAcceso).HasColumnName("clave_acceso").HasMaxLength(49).IsRequired();
        builder.Property(r => r.FechaEmision).HasColumnName("fecha_emision").IsRequired();
        builder.Property(r => r.TipoIdentificacionSujeto).HasColumnName("tipo_identificacion_sujeto").HasMaxLength(2).IsRequired();
        builder.Property(r => r.IdentificacionSujeto).HasColumnName("identificacion_sujeto").HasMaxLength(20).IsRequired();
        builder.Property(r => r.RazonSocialSujeto).HasColumnName("razon_social_sujeto").IsRequired();
        builder.Property(r => r.DireccionSujeto).HasColumnName("direccion_sujeto");
        builder.Property(r => r.PeriodoFiscal).HasColumnName("periodo_fiscal").HasMaxLength(7).IsRequired();
        builder.Property(r => r.TotalBaseImponible).HasColumnName("total_base_imponible").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(r => r.TotalRetencionRenta).HasColumnName("total_retencion_renta").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(r => r.TotalRetencionIva).HasColumnName("total_retencion_iva").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(r => r.TotalRetenido).HasColumnName("total_retenido").HasColumnType("numeric(12,2)").IsRequired();

        builder.Property(r => r.InfoAdicional)
            .HasColumnName("info_adicional")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<InfoAdicional>>(v, (JsonSerializerOptions?)null) ?? new());

        builder.Property(r => r.EstadoSri).HasColumnName("estado_sri").HasMaxLength(30).HasConversion(ConvertersEfCore.EstadoSriConverter).IsRequired();
        builder.Property(r => r.EstadoCorreo).HasColumnName("estado_correo").HasMaxLength(20).HasConversion(ConvertersEfCore.EstadoCorreoConverter).IsRequired();
        builder.Property(r => r.NumeroAutorizacion).HasColumnName("numero_autorizacion").HasMaxLength(49);
        builder.Property(r => r.FechaAutorizacion).HasColumnName("fecha_autorizacion");
        builder.Property(r => r.SriRespuesta).HasColumnName("sri_respuesta").HasColumnType("jsonb");
        builder.Property(r => r.XmlFirmadoPath).HasColumnName("xml_firmado_path");
        builder.Property(r => r.XmlAutorizadoPath).HasColumnName("xml_autorizado_path");
        builder.Property(r => r.PdfPath).HasColumnName("pdf_path");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(r => r.ClaveAcceso).IsUnique();
        builder.HasIndex(r => new { r.EmpresaRuc, r.EstadoSri, r.FechaEmision });

        builder.HasMany(r => r.Detalle)
            .WithOne()
            .HasForeignKey(d => d.RetencionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
