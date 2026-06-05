using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Facturacion.Core.Entidades;
using Facturacion.Core.Interfaces.Repositorios;

namespace Facturacion.Api.Filters;

public sealed class IdempotencyFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var httpCtx = ctx.HttpContext;
        var key = httpCtx.Request.Headers["Idempotency-Key"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(key) || key.Length > 128)
            return await next(ctx);

        if (!Guid.TryParse(httpCtx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? httpCtx.User.FindFirst("sub")?.Value, out var cuentaId))
            return await next(ctx);

        var repo = httpCtx.RequestServices.GetRequiredService<IIdempotencyRepositorio>();

        var existing = await repo.ObtenerAsync(key, cuentaId, httpCtx.RequestAborted);
        if (existing is not null)
        {
            httpCtx.Response.Headers["X-Idempotency-Replayed"] = "true";
            var body = JsonSerializer.Deserialize<JsonElement>(existing.ResponseBody);
            return Results.Json(body, statusCode: existing.StatusCode);
        }

        var result = await next(ctx);

        if (result is IResult iResult)
            return new CapturingResult(iResult, key, cuentaId, repo);

        return result;
    }

    private sealed class CapturingResult(
        IResult inner,
        string key,
        Guid cuentaId,
        IIdempotencyRepositorio repo) : IResult
    {
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            var originalBody = httpContext.Response.Body;
            using var buffer = new MemoryStream();
            httpContext.Response.Body = buffer;

            try
            {
                await inner.ExecuteAsync(httpContext);

                buffer.Position = 0;
                var responseBody = await new StreamReader(buffer, Encoding.UTF8).ReadToEndAsync();

                try
                {
                    await repo.GuardarAsync(new IdempotencyRecord
                    {
                        Key = key,
                        CuentaId = cuentaId,
                        StatusCode = httpContext.Response.StatusCode,
                        ResponseBody = responseBody,
                        ExpiresAt = DateTime.UtcNow.AddHours(24)
                    });
                }
                catch
                {
                    // no romper la respuesta si el almacenamiento falla
                }

                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody);
            }
            finally
            {
                httpContext.Response.Body = originalBody;
            }
        }
    }
}
