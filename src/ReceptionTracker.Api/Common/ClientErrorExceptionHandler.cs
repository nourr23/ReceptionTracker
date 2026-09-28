using Microsoft.AspNetCore.Diagnostics;
using ReceptionTracker.Domain.Common;

namespace ReceptionTracker.Api.Common;

/// <summary>
/// Turns exceptions caused by the client into 4xx ProblemDetails:
/// <list type="bullet">
/// <item>invalid request (malformed JSON, missing required field…) → 400,</item>
/// <item>violated business rule → 409.</item>
/// </list>
/// Any other exception falls through to the default handler → 500 without internal details.
/// </summary>
internal sealed class ClientErrorExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            // Thrown by minimal APIs in Development (in Production they answer 400 directly).
            BadHttpRequestException badRequest => (badRequest.StatusCode, "Invalid request"),
            DomainException => (StatusCodes.Status409Conflict, "Business rule violated"),
            _ => (0, null)
        };

        if (title is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message
            }
        });
    }
}
