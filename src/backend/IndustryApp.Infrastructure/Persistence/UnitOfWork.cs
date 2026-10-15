using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Exceptions;
using IndustryApp.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IndustryApp.Infrastructure.Persistence;

/// <summary>
/// Değişen entity'lerden olayları toplar, tek transaction'da kaydeder, veritabanı çakışmalarını
/// ConflictException'a çevirir ve olayları yalnızca kayıt başarılıysa kuyruğa verir (PAT-X.1, NFR-R2).
/// </summary>
public sealed class UnitOfWork(AppDbContext db, IEventDispatcher dispatcher) : IUnitOfWork
{
    private const string UniqueViolationSqlState = "23505";

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        var sources = db.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();
        var events = sources.SelectMany(e => e.DomainEvents).ToList();

        int affected;
        try
        {
            affected = await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(ErrorCodes.ConcurrencyConflict, "Kayıt başka bir işlem tarafından değiştirildi.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolationSqlState })
        {
            throw new ConflictException(ErrorCodes.UniqueViolation, "Kayıt zaten mevcut veya çakışan bir işlem yapıldı.");
        }

        sources.ForEach(s => s.ClearDomainEvents());
        if (events.Count > 0)
            await dispatcher.EnqueueAsync(events, ct); // yalnızca kayıt başarılıysa

        return affected;
    }
}
