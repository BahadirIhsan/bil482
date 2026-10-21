using Serilog.Context;

namespace IndustryApp.WebApi.Middleware;

/// <summary>
/// Her isteğe bir korelasyon kimliği verir: gelen X-Correlation-Id geçerliyse kullanılır, yoksa üretilir.
/// Kimlik yanıt başlığına, log bağlamına ve ProblemDetails içindeki traceId alanına yazılır.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    private const int MaxLength = 64;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].ToString();
        if (!IsValid(correlationId))
            correlationId = context.TraceIdentifier;

        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
            await next(context);
    }

    // Dışarıdan gelen değer log'a yazıldığı için yalnızca güvenli karakterler kabul edilir.
    private static bool IsValid(string value) =>
        !string.IsNullOrEmpty(value)
        && value.Length <= MaxLength
        && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' or ':');
}
