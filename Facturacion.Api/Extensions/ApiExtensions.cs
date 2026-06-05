using System.Threading.RateLimiting;
using Facturacion.Api.Endpoints.Cuentas;
using Facturacion.Api.Endpoints.Empresas;
using Facturacion.Api.Endpoints.Facturas;
using Facturacion.Api.Endpoints.NotasCredito;
using Facturacion.Api.Endpoints.Parametros;
using Facturacion.Api.Endpoints.Retenciones;
using Facturacion.Api.Middleware;
using Facturacion.Core.CasosDeUso.Comun;
using Facturacion.Core.CasosDeUso.Cuentas;
using Facturacion.Core.CasosDeUso.Empresas;
using Facturacion.Core.CasosDeUso.Facturas;
using Facturacion.Core.CasosDeUso.NotasCredito;
using Facturacion.Core.CasosDeUso.Parametros;
using Facturacion.Core.CasosDeUso.Retenciones;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
                    .WithHeaders("Content-Type", "Authorization")
                    .WithMethods("GET", "POST", "PUT");
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
            options.AddPolicy("escritura", ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ctx.User.FindFirst("sub")?.Value
                        ?? ctx.Connection.RemoteIpAddress?.ToString()
                        ?? "anon",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 20,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
                return ValueTask.CompletedTask;
            };
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
            options.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date",
                Example = new Microsoft.OpenApi.Any.OpenApiString("2025-01-31")
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingresa el token JWT de Supabase"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database");

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddScoped<OrquestadorEmision>();
        services.AddScoped<OrquestadorReintento>();
        services.AddScoped<GenerarPreviewPdf>();
        services.AddScoped<ObtenerUrlDescarga>();
        services.AddScoped<EmitirFactura>();
        services.AddScoped<EmitirNotaCredito>();
        services.AddScoped<EmitirRetencion>();
        services.AddScoped<ReintentarEmisionFactura>();
        services.AddScoped<ReintentarEmisionNotaCredito>();
        services.AddScoped<ReintentarEmisionRetencion>();
        services.AddScoped<GuardarEmpresa>();
        services.AddScoped<ActualizarCertificado>();
        services.AddScoped<GuardarSecuencialSri>();
        services.AddScoped<GuardarParametrosFacturacion>();
        services.AddScoped<EliminarCuenta>();

        return services;
    }

    public static WebApplication UseApiCors(this WebApplication app)
    {
        app.UseCors(CorsPolicy);
        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/v1");
        v1.MapCuentasEndpoints();
        v1.MapEmpresasEndpoints();
        v1.MapFacturasEndpoints();
        v1.MapNotasCreditoEndpoints();
        v1.MapRetencionesEndpoints();
        v1.MapParametrosEndpoints();
        return app;
    }

    public static WebApplication UseSwaggerDocs(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}
