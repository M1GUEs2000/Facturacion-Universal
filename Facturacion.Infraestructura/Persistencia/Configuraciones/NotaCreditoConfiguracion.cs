using System.Text.Json;
using Facturacion.Core.Entidades;
using Facturacion.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class NotaCreditoConfiguracion : IEntityTypeConfiguration<NotaCredito>
{
    static readonly ValueConverter<Ambiente, string> AmbienteConverter = new(
        v => ((int)v).ToString(),
        v => (Ambiente)int.Parse(v));

    static readonly ValueConverter<EstadoSri, string> EstadoSriConverter = new(
        v => v switch
        {
            EstadoSri.Enviado => "ENVIADO",
            EstadoSri.PendienteAutorizacion => "PENDIENTE_AUTORIZACION",
            EstadoSri.Autorizado => "AUTORIZADO",
            EstadoSri.NoAutorizado => "NO_AUTORIZADO",
            EstadoSri.Anulado => "ANULADO",
            _ => "PENDIENTE"
        },
        v => v switch
        {
            "ENVIADO" => EstadoSri.Enviado,
            "PENDIENTE_AUTORIZACION" => EstadoSri.PendienteAutorizacion,
            "AUTORIZADO" => EstadoSri.Autorizado,
            "NO_AUTORIZADO" => EstadoSri.NoAutorizado,
            "ANULADO" => EstadoSri.Anulado,
            _ => EstadoSri.Pendiente
        });

    static readonly ValueConverter<EstadoCorreo, string> EstadoCorreoConverter = new(
        v => v == EstadoCorreo.Enviado ? "ENVIADO" : "PENDIENTE",
        v => v == "ENVIADO" ? EstadoCorreo.Enviado : EstadoCorreo.Pendiente);

    public void Configure(EntityTypeBuilder<NotaCredito> builder)
    {
        builder.ToTable("notas_credito");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.EmpresaRuc).HasColumnName("empresa_ruc").HasMaxLength(13).IsRequired();
        builder.Property(n => n.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(n => n.Ambiente).HasColumnName("ambiente").HasMaxLength(1).HasConversion(AmbienteConverter).IsRequired();
        builder.Property(n => n.Estab).HasColumnName("estab").HasMaxLength(3).IsRequired();
        builder.Property(n => n.PtoEmi).HasColumnName("pto_emi").HasMaxLength(3).IsRequired();
        builder.Property(n => n.Secuencial).HasColumnName("secuencial").HasMaxLength(9).IsRequired();
        builder.Property(n => n.ClaveAcceso).HasColumnName("clave_acceso").HasMaxLength(49).IsRequired();
        builder.Property(n => n.FechaEmision).HasColumnName("fecha_emision").IsRequired();
        builder.Property(n => n.TipoIdentificacionComprador).HasColumnName("tipo_identificacion_comprador").HasMaxLength(2).IsRequired();
        builder.Property(n => n.IdentificacionComprador).HasColumnName("identificacion_comprador").HasMaxLength(20).IsRequired();
        builder.Property(n => n.RazonSocialComprador).HasColumnName("razon_social_comprador").IsRequired();
        builder.Property(n => n.DireccionComprador).HasColumnName("direccion_comprador");
        builder.Property(n => n.DirEstablecimiento).HasColumnName("dir_establecimiento");
        builder.Property(n => n.DocModificadoTipo).HasColumnName("doc_modificado_tipo").HasMaxLength(2).IsRequired();
        builder.Property(n => n.DocModificadoNumero).HasColumnName("doc_modificado_numero").HasMaxLength(17).IsRequired();
        builder.Property(n => n.DocModificadoFecha).HasColumnName("doc_modificado_fecha").IsRequired();
        builder.Property(n => n.DocModificadoClaveAcceso).HasColumnName("doc_modificado_clave_acceso").HasMaxLength(49).IsRequired();
        builder.Property(n => n.Motivo).HasColumnName("motivo").IsRequired();
        builder.Property(n => n.TotalSinImpuestos).HasColumnName("total_sin_impuestos").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(n => n.TotalDescuento).HasColumnName("total_descuento").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(n => n.BaseImponibleIce).HasColumnName("base_imponible_ice").HasColumnType("numeric(12,2)");
        builder.Property(n => n.ValorIce).HasColumnName("valor_ice").HasColumnType("numeric(12,2)");
        builder.Property(n => n.BaseImponibleIva).HasColumnName("base_imponible_iva").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(n => n.ValorIva).HasColumnName("valor_iva").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(n => n.ValorModificacion).HasColumnName("valor_modificacion").HasColumnType("numeric(12,2)").IsRequired();

        builder.Property(n => n.InfoAdicional)
            .HasColumnName("info_adicional")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<InfoAdicional>>(v, (JsonSerializerOptions?)null) ?? new());

        builder.Property(n => n.EstadoSri).HasColumnName("estado_sri").HasMaxLength(30).HasConversion(EstadoSriConverter).IsRequired();
        builder.Property(n => n.EstadoCorreo).HasColumnName("estado_correo").HasMaxLength(20).HasConversion(EstadoCorreoConverter).IsRequired();
        builder.Property(n => n.NumeroAutorizacion).HasColumnName("numero_autorizacion").HasMaxLength(49);
        builder.Property(n => n.FechaAutorizacion).HasColumnName("fecha_autorizacion");
        builder.Property(n => n.SriRespuesta).HasColumnName("sri_respuesta").HasColumnType("jsonb");
        builder.Property(n => n.XmlFirmadoPath).HasColumnName("xml_firmado_path");
        builder.Property(n => n.XmlAutorizadoPath).HasColumnName("xml_autorizado_path");
        builder.Property(n => n.PdfPath).HasColumnName("pdf_path");
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(n => n.ClaveAcceso).IsUnique();
        builder.HasIndex(n => new { n.EmpresaRuc, n.EstadoSri, n.FechaEmision });

        builder.HasMany(n => n.Detalle)
            .WithOne()
            .HasForeignKey(d => d.NotaCreditoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
