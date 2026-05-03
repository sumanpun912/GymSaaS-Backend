using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.Api.Common.Http;

/// <summary>
/// Translates domain-level <see cref="ErrorOr{TValue}"/> into RFC 7807 <see cref="ProblemDetails"/> HTTP responses.
/// Use this at the outer API edge so controllers/minimal endpoints do not branch on dozens of statuses.
/// </summary>
public static class ErrorOrHttpExtensions
{
    public static IResult ToHttpResult<T>(
        this ErrorOr<T> result,
        Func<T, IResult>? onSuccess = default)
    {
        if (result.IsError)
        {
            var problem = ToProblemDetails(result.Errors);
            return TypedResults.Problem(problem);
        }

        return onSuccess is not null ? onSuccess(result.Value) : TypedResults.Ok(result.Value);
    }

    /// <summary>Maps <see cref="ErrorOr{TValue}"/> to <see cref="IActionResult"/> for traditional MVC controllers.</summary>
    public static IActionResult ToActionResult<T>(
        this ErrorOr<T> result,
        Func<T, IActionResult>? onSuccess = default)
    {
        if (result.IsError)
        {
            var problem = ToProblemDetails(result.Errors);
            return new ObjectResult(problem) { StatusCode = problem.Status };
        }

        return onSuccess is not null ? onSuccess(result.Value) : new OkObjectResult(result.Value);
    }

    private static ProblemDetails ToProblemDetails(List<Error> errors)
    {
        var primary = errors[0];
        var status = MapStatus(primary.Type);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = MapTitle(primary.Type),
            Detail = primary.Description,
            Type = "about:blank"
        };

        problem.Extensions["code"] = primary.Code;
        problem.Extensions["errors"] = errors
            .Select(static e => new { e.Code, e.Description, type = e.Type.ToString() })
            .ToList();

        return problem;
    }

    private static string MapTitle(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.NotFound => "Resource not found",
            ErrorType.Validation => "Validation failed",
            ErrorType.Conflict => "Conflict",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.Failure => "Request failed",
            ErrorType.Unexpected => "Unexpected error",
            _ => "Something went wrong"
        };
    }

    private static int MapStatus(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
