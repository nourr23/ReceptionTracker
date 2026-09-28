using Microsoft.AspNetCore.Http.HttpResults;
using ReceptionTracker.Application.Common;

namespace ReceptionTracker.Api.Common;

/// <summary>Translates application results into HTTP responses (RFC 9457 ProblemDetails for errors).</summary>
internal static class ResultExtensions
{
    public static Results<Ok<T>, ProblemHttpResult> ToOkOrProblem<T>(this Result<T> result) =>
        result.Match<Results<Ok<T>, ProblemHttpResult>>(
            value => TypedResults.Ok(value),
            error => error.ToProblem());

    public static ProblemHttpResult ToProblem(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return TypedResults.Problem(
            statusCode: statusCode,
            detail: error.Message,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
