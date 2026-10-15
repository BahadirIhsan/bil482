using IndustryApp.Domain.Common;
using IndustryApp.WebApi.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Extensions;

public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Tüm hata yanıtlarını tek biçime sabitler: ProblemDetails + code + traceId (T-11.4).
    /// Model bağlama hataları da VALIDATION_FAILED koduyla döner.
    /// </summary>
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
            ctx.ProblemDetails.Extensions.TryAdd("code", null);
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Gönderilen bilgiler geçersiz."
            };
            problem.Extensions["code"] = ErrorCodes.ValidationFailed;
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        });

        return services;
    }
}
