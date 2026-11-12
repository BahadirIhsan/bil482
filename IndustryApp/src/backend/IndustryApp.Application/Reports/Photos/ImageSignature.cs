namespace IndustryApp.Application.Reports.Photos;

/// <summary>Kabul edilen görsel biçimleri; karar uzantıya veya Content-Type'a değil dosya imzasına göre verilir.</summary>
public enum ImageKind
{
    Jpeg,
    Png,
    WebP
}

public static class ImageSignature
{
    /// <summary>İmzayı okumak için gereken en az bayt sayısı.</summary>
    public const int HeaderLength = 12;

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>JPEG (FF D8 FF), PNG (89 50 4E 47 0D 0A 1A 0A) veya WebP (RIFF....WEBP); diğerleri null.</summary>
    public static ImageKind? Detect(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return ImageKind.Jpeg;
        if (header.Length >= Png.Length && header[..Png.Length].SequenceEqual(Png))
            return ImageKind.Png;
        if (header.Length >= HeaderLength
            && header[..4].SequenceEqual("RIFF"u8)
            && header[8..12].SequenceEqual("WEBP"u8))
            return ImageKind.WebP;
        return null;
    }
}
