using Facturacion.Api.Extensions;
using Facturacion.Infraestructura;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration));

    builder.Services
        .AddInfraestructura(builder.Configuration)
        .AddApi(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
        app.UseSwaggerDocs();

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
