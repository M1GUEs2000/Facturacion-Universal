using Facturacion.Api.Extensions;
using Facturacion.Core.CasosDeUso.Cuentas;

namespace Facturacion.Api.Endpoints.Cuentas;

public static class CuentasEndpoints
{
    public static WebApplication MapCuentasEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/cuenta")
            .WithTags("Cuenta")
            .RequireAuthorization()
            .RequireRateLimiting("escritura");

        group.MapDelete("", Eliminar).WithName("EliminarCuenta");

        return app;
    }

    private static async Task<IResult> Eliminar(
        EliminarCuenta useCase,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.User.FindFirst("sub")?.Value, out var cuentaJwtId))
            return Results.Unauthorized();

        var result = await useCase.EjecutarAsync(cuentaJwtId, cuentaJwtId, ct);
        return result.Match(
            _ => Results.NoContent(),
            errors => errors.ToProblemResult());
    }
}
