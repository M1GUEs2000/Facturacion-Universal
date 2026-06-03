using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Facturacion.Infraestructura.Persistencia.Repositorios;
using Facturacion.Infraestructura.Seguridad;
using Facturacion.Infraestructura.Servicios.Firma;
using Facturacion.Infraestructura.Servicios.Pdf;
using Facturacion.Infraestructura.Servicios.Sri;
using Facturacion.Infraestructura.Servicios.Storage;
using Facturacion.Infraestructura.Servicios.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infraestructura;

public static class InfraestructuraExtensions
{
    public static IServiceCollection AddInfraestructura(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Cifrado de CertPassword — clave de 32 bytes en base64 desde secrets manager
        var certKeyBase64 = configuration["Encryption:CertPasswordKey"];
        if (!string.IsNullOrWhiteSpace(certKeyBase64))
            CertPasswordEncryption.Initialize(Convert.FromBase64String(certKeyBase64));

        // Persistencia
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICuentasRepositorio, CuentasRepositorio>();
        services.AddScoped<IEmpresasRepositorio, EmpresasRepositorio>();
        services.AddScoped<IFacturasRepositorio, FacturasRepositorio>();
        services.AddScoped<INotasCreditoRepositorio, NotasCreditoRepositorio>();
        services.AddScoped<IRetencionesRepositorio, RetencionesRepositorio>();
        services.AddScoped<ISecuencialesSriRepositorio, SecuencialesSriRepositorio>();
        services.AddScoped<IParametrosFacturacionRepositorio, ParametrosFacturacionRepositorio>();

        // Servicios
        services.AddScoped<IServicioXml, ServicioXml>();
        services.AddScoped<IServicioFirma, ServicioFirma>();
        services.AddScoped<IServicioPdf, ServicioPdf>();

        // SRI HttpClient
        services.AddHttpClient("sri");
        services.AddScoped<IServicioSri, ServicioSri>();

        // Storage Supabase
        services.AddHttpClient("supabase-storage");

        var docsOpts = configuration.GetSection(SupabaseStorageOpciones.Seccion).Get<SupabaseStorageOpciones>()
            ?? new SupabaseStorageOpciones();
        var firmaOpts = configuration.GetSection(SupabaseStorageFirmaYLogoOpciones.Seccion).Get<SupabaseStorageFirmaYLogoOpciones>()
            ?? new SupabaseStorageFirmaYLogoOpciones();

        var firmaOptsAdaptado = new SupabaseStorageOpciones
        {
            Url = firmaOpts.Url,
            ServiceRoleKey = firmaOpts.ServiceRoleKey,
            Bucket = firmaOpts.Bucket
        };

        services.AddScoped<IServicioStorage>(sp => new ServicioStorageSupabase(
            sp.GetRequiredService<IHttpClientFactory>(),
            docsOpts,
            sp.GetRequiredService<ILogger<ServicioStorageSupabase>>()));

        services.AddScoped<IServicioStorageFirmaYLogo>(sp => new ServicioStorageSupabase(
            sp.GetRequiredService<IHttpClientFactory>(),
            firmaOptsAdaptado,
            sp.GetRequiredService<ILogger<ServicioStorageSupabase>>()));

        return services;
    }
}
