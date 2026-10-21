using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Reports;

/// <summary>Rapora bağlı, işlenmiş (EXIF/GPS temizlenmiş) fotoğraf kaydı. Dosya IFileStorage'dadır.</summary>
public sealed class ReportPhoto : BaseEntity<Guid>
{
    private ReportPhoto()
    {
    }

    internal ReportPhoto(Guid id, Guid reportId, string storageKey, PhotoMetadata meta, short sortOrder, DateTimeOffset now)
    {
        Id = id;
        ReportId = reportId;
        StorageKey = storageKey;
        OriginalFileName = meta.OriginalFileName;
        ContentType = meta.ContentType;
        SizeBytes = meta.SizeBytes;
        Width = meta.Width;
        Height = meta.Height;
        SortOrder = sortOrder;
        CreatedAt = now;
    }

    public Guid ReportId { get; private set; }

    public string StorageKey { get; private set; } = default!;

    public string? OriginalFileName { get; private set; }

    public string ContentType { get; private set; } = default!;

    public int SizeBytes { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public short SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
