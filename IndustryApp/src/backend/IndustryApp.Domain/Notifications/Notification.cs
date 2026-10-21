using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Notifications;

/// <summary>Kullanıcıya düşen bildirim. Kullanıcı çevrimdışıyken de kaybolmaması için önce bu kayıt yazılır.</summary>
public sealed class Notification : BaseEntity<Guid>
{
    private Notification()
    {
    }

    public Guid UserId { get; private set; }

    public string Type { get; private set; } = default!;

    public string Title { get; private set; } = default!;

    public string Body { get; private set; } = default!;

    /// <summary>jsonb; ör. {"reportId":"…","offerId":"…"} → tıklanınca ilgili ekrana gidilir.</summary>
    public string? Payload { get; private set; }

    public bool IsRead { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public static Notification Create(Guid userId, string type, string title, string body, string? payloadJson, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Bildirim türü boş olamaz.", nameof(type));

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            Payload = payloadJson,
            IsRead = false,
            CreatedAt = now
        };
    }

    public void MarkRead(DateTimeOffset now)
    {
        if (IsRead)
            return;
        IsRead = true;
        ReadAt = now;
    }
}
