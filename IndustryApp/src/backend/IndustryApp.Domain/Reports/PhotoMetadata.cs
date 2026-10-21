namespace IndustryApp.Domain.Reports;

/// <summary>İşlenmiş fotoğrafın rapora yazılan bilgileri.</summary>
public sealed record PhotoMetadata(string ContentType, int SizeBytes, int Width, int Height, string? OriginalFileName = null);
