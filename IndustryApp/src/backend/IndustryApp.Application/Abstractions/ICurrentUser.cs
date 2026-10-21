namespace IndustryApp.Application.Abstractions;

/// <summary>İsteği yapan kullanıcı. Web'de JWT'den, arka plan işlerinde sistem kullanıcısından okunur.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);

    string? IpAddress { get; }
}
