using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Accounts;

/// <summary>Sürümlü sözleşme metni (KVKK, kullanım koşulları). Referans veridir; ekrandan oluşturulmaz.</summary>
public sealed class LegalDocument : BaseEntity<int>
{
    /// <summary>target_role değeri: tüm rollere uygulanan belge.</summary>
    public const string AllRoles = "All";

    private LegalDocument()
    {
    }

    public string Code { get; private set; } = default!;

    public string Version { get; private set; } = default!;

    public string TargetRole { get; private set; } = default!;

    public string Title { get; private set; } = default!;

    public string Body { get; private set; } = default!;

    public DateTimeOffset PublishedAt { get; private set; }

    public bool IsCurrent { get; private set; }

    public bool AppliesTo(string role) => TargetRole == AllRoles || TargetRole == role;
}
