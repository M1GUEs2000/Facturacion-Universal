using Facturacion.Api.Endpoints.Empresas;
using Facturacion.Api.Endpoints.Facturas;
using Facturacion.Api.Endpoints.NotasCredito;
using Facturacion.Api.Endpoints.Retenciones;
using Facturacion.Api.Middleware;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.Empresas;
using Facturacion.Core.CasosDeUso.Facturas;
using Facturacion.Core.CasosDeUso.NotasCredito;
using Facturacion.Core.CasosDeUso.Retenciones;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Facturacion.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = config["Jwt:Authority"];
                options.Audience = config["Jwt:Audience"];
            });
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(t => t.FullName);
            options.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
            {
                Type = "string",
                Format = "date",
                Example = new Microsoft.OpenApi.Any.OpenApiString("2025-01-31")
            });
        });
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddScoped<OrquestadorEmision>();
        services.AddScoped<EmitirFactura>();
        services.AddScoped<EmitirNotaCredito>();
        services.AddScoped<EmitirRetencion>();
        services.AddScoped<RegistrarEmpresa>();
        services.AddScoped<ActualizarCertificado>();

        return services;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapEmpresasEndpoints();
        app.MapFacturasEndpoints();
        app.MapNotasCreditoEndpoints();
        app.MapRetencionesEndpoints();
        return app;
    }

    public static WebApplication UseSwaggerDocs(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}
