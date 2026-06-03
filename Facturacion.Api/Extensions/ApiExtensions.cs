using System.Threading.RateLimiting;
using Facturacion.Api.Endpoints.Empresas;
using Facturacion.Api.Endpoints.Facturas;
using Facturacion.Api.Endpoints.NotasCredito;
using Facturacion.Api.Endpoints.Parametros;
using Facturacion.Api.Endpoints.Retenciones;
using Facturacion.Api.Middleware;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.Empresas;
using Facturacion.Core.CasosDeUso.Facturas;
using Facturacion.Core.CasosDeUso.NotasCredito;
using Facturacion.Core.CasosDeUso.Parametros;
using Facturacion.Core.CasosDeUso.Retenciones;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace Facturacion.Api.Extensions;

public static class ApiExtensions
{
    private const string CorsPolicy = "CrmUniversal";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicy, policy =>
            {
                var origins = config.GetSection("Cors:Origins").Get<string[]>()
                    ?? ["http://localhost:5173", "http://127.0.0.1:5173"];
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddRateLimiter(options =>
        {
            options.AddPolicy("emision", ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ctx.User.FindFirst("sub")?.Value
                        ?? ctx.Connection.RemoteIpAddress?.ToString()
                        ?? "anon",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 60,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = config["Jwt:Authority"];
                options.Audience = config["Jwt:Audience"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 }
                };
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
        services.AddScoped<OrquestadorReintento>();
        services.AddScoped<GenerarPreviewPdf>();
        services.AddScoped<EmitirFactura>();
        services.AddScoped<EmitirNotaCredito>();
        services.AddScoped<EmitirRetencion>();
        services.AddScoped<ReintentarEmisionFactura>();
        services.AddScoped<ReintentarEmisionNotaCredito>();
        services.AddScoped<ReintentarEmisionRetencion>();
        services.AddScoped<GuardarEmpresa>();
        services.AddScoped<RegistrarEmpresa>();
        services.AddScoped<ActualizarEmpresa>();
        services.AddScoped<ActualizarCertificado>();
        services.AddScoped<GuardarSecuencialSri>();
        services.AddScoped<GuardarParametrosFacturacion>();

        return services;
    }

    public static WebApplication UseApiCors(this WebApplication app)
    {
        app.UseCors(CorsPolicy);
        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapEmpresasEndpoints();
        app.MapFacturasEndpoints();
        app.MapNotasCreditoEndpoints();
        app.MapRetencionesEndpoints();
        app.MapParametrosEndpoints();
        return app;
    }

    public static WebApplication UseSwaggerDocs(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}
