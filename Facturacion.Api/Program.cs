using System.Diagnostics;
using System.Net;
using Facturacion.Api.Extensions;
using Facturacion.Infraestructura;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .Destructure.With<SensitiveDataDestructuringPolicy>()
        .ReadFrom.Configuration(ctx.Configuration));

    builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 1_048_576);

    builder.Services
        .AddInfraestructura(builder.Configuration)
        .AddApi(builder.Configuration);

    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
    });

    var app = builder.Build();

    // Solo habilitar si la app está detrás de un reverse proxy conocido.
    // Configurar ForwardedHeaders:KnownProxyIp en secrets/env para activar.
    var proxyIp = app.Configuration["ForwardedHeaders:KnownProxyIp"];
    if (!string.IsNullOrWhiteSpace(proxyIp) && IPAddress.TryParse(proxyIp, out var proxyAddress))
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            KnownProxies = { proxyAddress }
        });
    }

    app.UseExceptionHandler();
    app.UseRateLimiter();

    app.Use(async (ctx, next) =>
    {
        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Response.Headers["X-Frame-Options"] = "DENY";
        ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
        ctx.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        ctx.Response.Headers["X-Correlation-Id"] =
            Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        await next();
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    app.UseApiCors();
    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
        app.UseSwaggerDocs();

    app.MapHealthChecks("/health");
    app.MapApiEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminado inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
