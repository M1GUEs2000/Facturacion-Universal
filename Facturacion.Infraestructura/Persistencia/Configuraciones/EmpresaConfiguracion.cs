using Facturacion.Core.Entidades;
using Facturacion.Infraestructura.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Facturacion.Infraestructura.Persistencia.Configuraciones;

public class EmpresaConfiguracion : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");

        builder.HasKey(e => e.Ruc);

        builder.Property(e => e.Ruc).HasColumnName("ruc").HasMaxLength(13).IsRequired();
        builder.Property(e => e.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(e => e.DirMatriz).HasColumnName("dir_matriz").IsRequired();
        builder.Property(e => e.NombreComercial).HasColumnName("nombre_comercial");
        builder.Property(e => e.LogoPath).HasColumnName("logo_path").HasMaxLength(500);
        builder.Property(e => e.LogoContentType).HasColumnName("logo_content_type").HasMaxLength(100);
        builder.Property(e => e.CertificadoPath).HasColumnName("certificado_path").HasMaxLength(500).IsRequired();
        builder.Property(e => e.CertPassword)
            .HasColumnName("cert_password")
            .IsRequired()
            .HasConversion(
                v => CertPasswordEncryption.Encrypt(v),
                v => CertPasswordEncryption.Decrypt(v));
        builder.Property(e => e.CuentaId).HasColumnName("cuenta_id");
        builder.HasOne(e => e.Cuenta).WithMany().HasForeignKey(e => e.CuentaId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
