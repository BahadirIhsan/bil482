using FluentValidation;
using IndustryApp.Application.Common.Exceptions;
using IndustryApp.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Middleware;

/// <summary>
/// Yakalanmamış exception'ları RFC 7807 ProblemDetails'e çevirir (T-11.4):
/// DomainException → 409 + kendi kodu, ConflictException → 409, ValidationException → 400 + alan hataları,
/// diğerleri → 500 (iç ayrıntı gösterilmez, traceId ile loglanır).
/// </summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> log)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, code, title, errors) = exception switch
        {
            DomainException e => (StatusCodes.Status409Conflict, e.Code, e.Message, null),
            ConflictException e => (StatusCodes.Status409Conflict, e.Code, e.Message, null),
            ValidationException e => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationFailed,
                "Gönderilen bilgiler geçersiz.",
                e.Errors
                    .GroupBy(f => ToCamelCase(f.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray())),
            _ => (StatusCodes.Status500InternalServerError, ErrorCodes.Unexpected,
                "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.", (Dictionary<string, string[]>?)null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            log.LogError(exception, "İşlenmeyen hata. TraceId: {TraceId}", context.TraceIdentifier);
        else
            log.LogInformation("İş kuralı hatası {Code}: {Message}", code, exception.Message);

        var problem = new ProblemDetails { Status = status, Title = title };
        problem.Extensions["code"] = code;
        problem.Extensions["errors"] = errors;

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
