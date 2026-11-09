using System.Text.Json;
using System.Text.Json.Serialization;
using IndustryApp.Application;
using IndustryApp.Application.Abstractions;
using IndustryApp.Infrastructure;
using IndustryApp.WebApi.Extensions;
using IndustryApp.WebApi.Filters;
using IndustryApp.WebApi.Middleware;
using IndustryApp.WebApi.Realtime;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Sink'ler (console + logs/api-.log günlük dosya) ve seviyeler appsettings "Serilog" bölümünden okunur.
    builder.Host.UseSerilog((context, services, logger) => logger
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddAppOptions(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApiAuthentication(builder.Configuration);
    builder.Services.AddApiRateLimiting(builder.Configuration);

    // REST ve SignalR yanıtları camelCase'e sabitlenir; enum'lar metin olarak döner (T-14.5).
    builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>()).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    builder.Services.AddSignalR().AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    builder.Services.AddSingleton<IRealtimePublisher, SignalRRealtimePublisher>();
    builder.Services.AddApiProblemDetails();
    builder.Services.AddApiSwagger();
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString(IndustryApp.Infrastructure.DependencyInjection.ConnectionStringName)!, name: "database");

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<NotificationHub>(NotificationHub.Path);
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Uygulama başlatılamadı");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Entegrasyon testlerindeki WebApplicationFactory için.</summary>
public partial class Program;
