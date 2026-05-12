using Facturacion.Core.Interfaces.Repositorios;
using Facturacion.Core.Interfaces.Servicios;
using Facturacion.Infraestructura.Persistencia.Contexto;
using Facturacion.Infraestructura.Persistencia.Repositorios;
using Facturacion.Infraestructura.Servicios.Firma;
using Facturacion.Infraestructura.Servicios.Pdf;
using Facturacion.Infraestructura.Servicios.Sri;
using Facturacion.Infraestructura.Servicios.Storage;
using Facturacion.Infraestructura.Servicios.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Facturacion.Infraestructura;

public static class InfraestructuraExtensions
{
    public static IServiceCollection AddInfraestructura(
        this IServiceCollection services, IConfiguration configuration)
    {
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

        // Storage local
        services.Configure<StorageLocalOpciones>(
            configuration.GetSection(StorageLocalOpciones.Seccion));
        services.AddScoped<IServicioStorage, ServicioStorageLocal>();

        return services;
    }
}
