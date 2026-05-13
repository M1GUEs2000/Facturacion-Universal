using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Excepción no manejada");

        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 500,
            Title = exception.GetType().Name,
            Detail = exception.Message
        }, ct);

        return true;
    }
}
