using ErrorOr;

namespace Facturacion.Api.Extensions;

public static class ErrorOrExtensions
{
    public static IResult ToProblemResult(this List<Error> errors)
    {
        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            var dict = errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            return Results.ValidationProblem(dict);
        }

        var first = errors.First();
        var status = first.Type switch
        {
            ErrorType.NotFound     => StatusCodes.Status404NotFound,
            ErrorType.Conflict     => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Validation   => StatusCodes.Status400BadRequest,
            ErrorType.Failure      => StatusCodes.Status422UnprocessableEntity,
            _                      => StatusCodes.Status500InternalServerError
        };

        var extras = errors.Count > 1
            ? new Dictionary<string, object?> { ["errors"] = errors.Skip(1).Select(e => new { e.Code, e.Description }) }
            : null;

        return Results.Problem(detail: first.Description, title: first.Code, statusCode: status, extensions: extras);
    }
}
