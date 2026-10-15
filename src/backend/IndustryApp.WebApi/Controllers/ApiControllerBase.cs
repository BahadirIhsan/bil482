using IndustryApp.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>
/// Tüm controller'ların tabanı. Yalnızca Result → HTTP yanıtı dönüşümü vardır; iş mantığı yoktur (T-11.1).
/// Controller'lar birbirini çağırmaz, DbContext veya Infrastructure sınıfı kullanmaz.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToActionResult(Result result) =>
        result.IsSuccess ? NoContent() : ToProblem(result.Error!);

    protected IActionResult ToActionResult<T>(Result<T> result, Func<T, IActionResult>? onSuccess = null) =>
        result.IsSuccess ? onSuccess?.Invoke(result.Value) ?? Ok(result.Value) : ToProblem(result.Error!);

    private ObjectResult ToProblem(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status422UnprocessableEntity
        };

        var problem = new ProblemDetails { Status = status, Title = error.Message };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return new ObjectResult(problem) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }
}
