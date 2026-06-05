using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Cuentas;
using Facturacion.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Endpoints.Cuentas;

public static class CuentasEndpoints
{
    public static IEndpointRouteBuilder MapCuentasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cuenta")
            .WithTags("Cuenta")
            .RequireAuthorization()
            .RequireRateLimiting("escritura");

        group.MapDelete("", Eliminar).WithName("EliminarCuenta");

        return app;
    }

    private static async Task<IResult> Eliminar(
        [FromServices] EliminarCuenta useCase,
        [FromServices] IAuditLogger audit,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaJwtId))
            return Results.Unauthorized();

        var result = await useCase.EjecutarAsync(cuentaJwtId, cuentaJwtId, ct);
        audit.Registrar(new EventoAudit(
            Tipo: EventosAudit.CuentaEliminada,
            CuentaId: cuentaJwtId,
            Ip: ctx.Connection.RemoteIpAddress?.ToString(),
            Exito: !result.IsError,
            CodigoError: result.IsError ? result.FirstError.Code : null));
        return result.Match(
            _ => Results.NoContent(),
            errors => errors.ToProblemResult());
    }
}
