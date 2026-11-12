namespace IndustryApp.Infrastructure.Imaging;

/// <summary>
/// JPEG dosyasındaki meta veri bölümleri: APP1 (EXIF, GPS dahil, ve XMP) ile APP13 (IPTC). Skia kodlarken meta veri
/// yazmadığı için hat bu bölümleri kendisi taşır; ExifStripProcessor onları boşaltır.
/// </summary>
internal static class JpegMetadataSegments
{
    private const byte App1 = 0xE1;
    private const byte App13 = 0xED;
    private const byte StartOfScan = 0xDA;
    private const byte EndOfImage = 0xD9;

    /// <summary>Kaynak JPEG ise meta veri bölümlerini (işaretçi dahil) sırayla döner; değilse boş liste.</summary>
    public static IReadOnlyList<byte[]> Read(ReadOnlySpan<byte> jpeg)
    {
        var segments = new List<byte[]>();
        if (jpeg.Length < 4 || jpeg[0] != 0xFF || jpeg[1] != 0xD8)
            return segments;

        var i = 2;
        while (i + 4 <= jpeg.Length && jpeg[i] == 0xFF)
        {
            var marker = jpeg[i + 1];
            if (marker == 0xFF)
            {
                i++; // dolgu baytı
                continue;
            }

            if (marker is StartOfScan or EndOfImage)
                break;

            var length = (jpeg[i + 2] << 8) | jpeg[i + 3];
            if (length < 2 || i + 2 + length > jpeg.Length)
                break;

            if (marker is App1 or App13)
                segments.Add(jpeg.Slice(i, 2 + length).ToArray());
            i += 2 + length;
        }

        return segments;
    }

    /// <summary>Bölümleri SOI işaretçisinin hemen arkasına ekler.</summary>
    public static byte[] Insert(byte[] jpeg, IReadOnlyList<byte[]> segments)
    {
        if (segments.Count == 0)
            return jpeg;

        var result = new byte[jpeg.Length + segments.Sum(s => s.Length)];
        jpeg.AsSpan(0, 2).CopyTo(result);
        var offset = 2;
        foreach (var segment in segments)
        {
            segment.CopyTo(result, offset);
            offset += segment.Length;
        }

        jpeg.AsSpan(2).CopyTo(result.AsSpan(offset));
        return result;
    }
}
