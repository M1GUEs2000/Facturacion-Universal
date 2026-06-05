using Facturacion.Core.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infraestructura.Persistencia.Contexto;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FacturaDetalle> FacturasDetalle => Set<FacturaDetalle>();
    public DbSet<NotaCredito> NotasCredito => Set<NotaCredito>();
    public DbSet<NotaCreditoDetalle> NotasCreditoDetalle => Set<NotaCreditoDetalle>();
    public DbSet<Retencion> Retenciones => Set<Retencion>();
    public DbSet<RetencionDetalle> RetencionesDetalle => Set<RetencionDetalle>();
    public DbSet<SecuencialSri> SecuencialesSri => Set<SecuencialSri>();
    public DbSet<ParametrosFacturacion> ParametrosFacturacion => Set<ParametrosFacturacion>();
    public DbSet<IdempotencyRecord> IdempotencyKeys => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("facturacion");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
