using Facturacion.Core.Entidades;
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
        builder.Property(e => e.ObligadoContabilidad).HasColumnName("obligado_contabilidad").IsRequired();
        builder.Property(e => e.CertificadoP12).HasColumnName("certificado_p12").IsRequired();
        builder.Property(e => e.CertPassword).HasColumnName("cert_password").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
