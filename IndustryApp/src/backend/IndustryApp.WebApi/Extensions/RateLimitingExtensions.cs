using System.Threading.RateLimiting;
using IndustryApp.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IndustryApp.WebApi.Extensions;

/// <summary>"RateLimiting" bölümü. Limitler koda gömülmez (NFR-M2).</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int LoginPerMinute { get; init; } = 5;

    public int RegisterPerMinute { get; init; } = 3;
}

public static class RateLimitingExtensions
{
    public const string LoginPolicy = "login";
    public const string RegisterPolicy = "register";

    /// <summary>Giriş ve kayıt için IP başına dakikalık sabit pencere (T-15). Aşımda 429 + TOO_MANY_REQUESTS.</summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(LoginPolicy, ctx => PerIp(ctx, limits.LoginPerMinute));
            options.AddPolicy(RegisterPolicy, ctx => PerIp(ctx, limits.RegisterPerMinute));
            options.OnRejected = async (ctx, ct) =>
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Çok fazla deneme yapıldı. Lütfen biraz sonra tekrar deneyin."
                };
                problem.Extensions["code"] = ErrorCodes.TooManyRequests;
                problem.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
                await ctx.HttpContext.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", ct);
            };
        });
        return services;
    }

    private static RateLimitPartition<string> PerIp(HttpContext ctx, int permitPerMinute) =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permitPerMinute, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 });
}
