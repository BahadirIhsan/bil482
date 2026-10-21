using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Reports;

/// <summary>Rapordaki hasar kalemi: hasar ağacından bir kategori ve şiddeti (FR-02.2).</summary>
public sealed class ReportDamageItem : BaseEntity<long>
{
    private ReportDamageItem()
    {
    }

    internal ReportDamageItem(Guid reportId, int damageCategoryId, DamageSeverity severity, string? note)
    {
        ReportId = reportId;
        DamageCategoryId = damageCategoryId;
        Severity = severity;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public Guid ReportId { get; private set; }

    public int DamageCategoryId { get; private set; }

    public DamageSeverity Severity { get; private set; }

    public string? Note { get; private set; }
}
