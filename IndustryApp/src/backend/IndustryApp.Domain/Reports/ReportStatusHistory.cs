using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Reports;

/// <summary>
/// Rapor durum geçmişi; aynı zamanda denetim kaydıdır. Seçim komutunun snapshot'ı burada saklanır
/// ve geri alma bu kayıttan komutu yeniden kurar (T-8.6).
/// </summary>
public sealed class ReportStatusHistory : BaseEntity<long>
{
    private ReportStatusHistory()
    {
    }

    public Guid ReportId { get; private set; }

    public ReportStatus? FromStatus { get; private set; }

    public ReportStatus ToStatus { get; private set; }

    /// <summary>Durumu değiştiren komut veya iş (SelectOfferCommand, UndoSelectOffer, ExpireReport ...).</summary>
    public string CommandName { get; private set; } = default!;

    /// <summary>İşlemi yapan kullanıcı; null ise sistem (arka plan işi).</summary>
    public Guid? ActorUserId { get; private set; }

    /// <summary>jsonb; geri alma için gereken önceki durum.</summary>
    public string? Snapshot { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public static ReportStatusHistory Create(
        Guid reportId,
        ReportStatus? fromStatus,
        ReportStatus toStatus,
        string commandName,
        Guid? actorUserId,
        ReportCommandSnapshot? snapshot,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("Komut adı boş olamaz.", nameof(commandName));

        return new ReportStatusHistory
        {
            ReportId = reportId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            CommandName = commandName,
            ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
            Snapshot = snapshot?.ToJson(),
            OccurredAt = now
        };
    }

    public ReportCommandSnapshot? ReadSnapshot() => Snapshot is null ? null : ReportCommandSnapshot.FromJson(Snapshot);
}
