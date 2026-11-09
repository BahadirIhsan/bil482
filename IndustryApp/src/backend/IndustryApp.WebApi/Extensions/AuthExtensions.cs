using System.Text;
using IndustryApp.Application.Common.Options;
using IndustryApp.Domain.Accounts;
using IndustryApp.Infrastructure.Identity;
using IndustryApp.WebApi.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace IndustryApp.WebApi.Extensions;

public static class AuthExtensions
{
    /// <summary>
    /// JWT doğrulaması ve politikalar (K-11). Claim adları token'daki gibi kalır (sub, role, mechanic_status);
    /// WebSocket başlık gönderemediği için /hubs isteklerinde token access_token sorgu parametresinden okunur.
    /// </summary>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Jwt ayarları tanımlı değil.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey ?? "")),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.CustomerOnly, p => p.RequireRole(RoleNames.Customer))
            .AddPolicy(AuthPolicies.MechanicAny, p => p.RequireRole(RoleNames.Mechanic))
            .AddPolicy(AuthPolicies.ApprovedMechanic, p => p
                .RequireRole(RoleNames.Mechanic)
                .RequireClaim(JwtTokenService.MechanicStatusClaim, nameof(MechanicApprovalStatus.Approved)))
            .AddPolicy(AuthPolicies.AdminOnly, p => p.RequireRole(RoleNames.Admin));

        // SignalR Clients.User(id) için kullanıcı kimliği sub claim'idir.
        services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
        return services;
    }
}
