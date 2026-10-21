using System.Text.Json;

namespace IndustryApp.Domain.Reports;

/// <summary>
/// Geri alınabilir bir rapor komutunun önceki durumu (T-8.6). report_status_history.snapshot
/// kolonunda jsonb olarak saklanır: {"selectedOfferId":"…","notSelectedOfferIds":["…"]}.
/// </summary>
public sealed record ReportCommandSnapshot(Guid SelectedOfferId, IReadOnlyList<Guid> NotSelectedOfferIds)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static ReportCommandSnapshot FromJson(string json) =>
        JsonSerializer.Deserialize<ReportCommandSnapshot>(json, JsonOptions)
        ?? throw new InvalidOperationException("Komut snapshot'ı okunamadı.");
}
