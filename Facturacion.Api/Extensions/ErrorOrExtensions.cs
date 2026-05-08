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
            _                      => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(detail: first.Description, title: first.Code, statusCode: status);
    }
}
