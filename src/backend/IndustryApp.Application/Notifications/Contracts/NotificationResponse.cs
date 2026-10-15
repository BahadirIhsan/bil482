namespace IndustryApp.Application.Notifications.Contracts;

/// <summary>Bildirim listesi satırı. Payload, ilgili ekrana yönlendirme için JSON metnidir.</summary>
public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? Payload,
    bool IsRead,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
