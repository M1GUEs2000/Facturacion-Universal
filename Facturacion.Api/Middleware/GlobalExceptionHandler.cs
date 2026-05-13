using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Excepción no manejada");

        var inner = exception;
        while (inner.InnerException is not null) inner = inner.InnerException;

        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 500,
            Title = exception.GetType().Name,
            Detail = inner == exception ? exception.Message : $"{exception.Message} → {inner.GetType().Name}: {inner.Message}"
        }, ct);

        return true;
    }
}
