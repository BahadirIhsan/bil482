using System.Collections.Concurrent;
using System.Linq.Expressions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IndustryApp.Infrastructure.Events;

/// <summary>
/// Olay kuyruğunu okur; her olay için yeni bir DI scope açar ve o olay türünün tüm IEventHandler&lt;T&gt;
/// implementasyonlarını çalıştırır. Bir handler hata verirse loglanır, diğerleri çalışmaya devam eder (T-9.1).
/// </summary>
public sealed class EventProcessingBackgroundService(
    InProcessEventDispatcher dispatcher,
    IServiceScopeFactory scopes,
    ILogger<EventProcessingBackgroundService> log) : BackgroundService
{
    private delegate Task HandlerInvoker(object handler, IDomainEvent domainEvent, CancellationToken ct);

    // Olay türü başına derlenmiş çağrı: her olayda yansıma (dynamic) maliyeti ödenmez.
    private static readonly ConcurrentDictionary<Type, (Type HandlerType, HandlerInvoker Invoke)> Invokers = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var domainEvent in dispatcher.Reader.ReadAllAsync(stoppingToken))
                await DispatchAsync(domainEvent, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Uygulama kapanıyor.
        }
    }

    private async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        var eventName = domainEvent.GetType().Name;
        var (handlerType, invoke) = Invokers.GetOrAdd(domainEvent.GetType(), CreateInvoker);

        using var scope = scopes.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType).OfType<object>().ToList();
        if (handlers.Count == 0)
        {
            log.LogDebug("{Event} için kayıtlı handler yok", eventName);
            return;
        }

        foreach (var handler in handlers)
        {
            try
            {
                await invoke(handler, domainEvent, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                log.LogError(ex, "{Event} işlenemedi: {Handler}", eventName, handler.GetType().Name);
            }
        }
    }

    private static (Type HandlerType, HandlerInvoker Invoke) CreateInvoker(Type eventType)
    {
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var method = handlerType.GetMethod(nameof(IEventHandler<IDomainEvent>.HandleAsync))!;

        var handler = Expression.Parameter(typeof(object), "handler");
        var domainEvent = Expression.Parameter(typeof(IDomainEvent), "domainEvent");
        var ct = Expression.Parameter(typeof(CancellationToken), "ct");
        var call = Expression.Call(
            Expression.Convert(handler, handlerType), method, Expression.Convert(domainEvent, eventType), ct);

        return (handlerType, Expression.Lambda<HandlerInvoker>(call, handler, domainEvent, ct).Compile());
    }
}
