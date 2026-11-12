using System.Buffers.Binary;
using SkiaSharp;

namespace IndustryApp.UnitTests.Infrastructure;

/// <summary>Testler için bellekte üretilen görseller; EXIF bölümü (GPS ve yön etiketi) elle yazılır.</summary>
internal static class TestImages
{
    public static readonly byte[] ExifMarker = "Exif\0\0"u8.ToArray();

    public static byte[] Png(int width, int height) => Encode(width, height, SKEncodedImageFormat.Png);

    public static byte[] Jpeg(int width, int height) => Encode(width, height, SKEncodedImageFormat.Jpeg);

    /// <summary>GPSLatitudeRef = "N" ve Orientation etiketli EXIF bölümü taşıyan JPEG (telefon fotoğrafı gibi).</summary>
    public static byte[] JpegWithGpsExif(int width, int height, ushort orientation = 1)
    {
        var jpeg = Jpeg(width, height);
        var app1 = ExifSegment(orientation);
        var result = new byte[jpeg.Length + app1.Length];
        jpeg.AsSpan(0, 2).CopyTo(result);
        app1.CopyTo(result, 2);
        jpeg.AsSpan(2).CopyTo(result.AsSpan(2 + app1.Length));
        return result;
    }

    public static bool Contains(byte[] data, byte[] pattern) => data.AsSpan().IndexOf(pattern) >= 0;

    public static (int Width, int Height) SizeOf(byte[] image)
    {
        using var codec = SKCodec.Create(new SKMemoryStream(image));
        return (codec.Info.Width, codec.Info.Height);
    }

    private static byte[] Encode(int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
            canvas.Clear(SKColors.SteelBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }

    /// <summary>APP1: "Exif\0\0" + küçük-uçlu TIFF; IFD0'da Orientation ve GPS IFD işaretçisi, GPS IFD'de GPSLatitudeRef.</summary>
    private static byte[] ExifSegment(ushort orientation)
    {
        var tiff = new byte[56];
        var t = tiff.AsSpan();
        "II"u8.CopyTo(t);
        BinaryPrimitives.WriteUInt16LittleEndian(t[2..], 42);
        BinaryPrimitives.WriteUInt32LittleEndian(t[4..], 8);

        // IFD0 (8): 2 kayıt + sonraki IFD = 30 bayt → GPS IFD 38'de.
        BinaryPrimitives.WriteUInt16LittleEndian(t[8..], 2);
        WriteEntry(t[10..], tag: 0x0112, type: 3, count: 1, value: orientation);
        WriteEntry(t[22..], tag: 0x8825, type: 4, count: 1, value: 38);
        BinaryPrimitives.WriteUInt32LittleEndian(t[34..], 0);

        // GPS IFD (38): GPSLatitudeRef = "N".
        BinaryPrimitives.WriteUInt16LittleEndian(t[38..], 1);
        BinaryPrimitives.WriteUInt16LittleEndian(t[40..], 0x0001);
        BinaryPrimitives.WriteUInt16LittleEndian(t[42..], 2);
        BinaryPrimitives.WriteUInt32LittleEndian(t[44..], 2);
        t[48] = (byte)'N';
        BinaryPrimitives.WriteUInt32LittleEndian(t[52..], 0);

        var payload = ExifMarker.Concat(tiff).ToArray();
        var segment = new byte[4 + payload.Length];
        segment[0] = 0xFF;
        segment[1] = 0xE1;
        BinaryPrimitives.WriteUInt16BigEndian(segment.AsSpan(2), (ushort)(2 + payload.Length));
        payload.CopyTo(segment, 4);
        return segment;
    }

    private static void WriteEntry(Span<byte> at, ushort tag, ushort type, uint count, uint value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(at, tag);
        BinaryPrimitives.WriteUInt16LittleEndian(at[2..], type);
        BinaryPrimitives.WriteUInt32LittleEndian(at[4..], count);
        BinaryPrimitives.WriteUInt32LittleEndian(at[8..], value);
    }
}
