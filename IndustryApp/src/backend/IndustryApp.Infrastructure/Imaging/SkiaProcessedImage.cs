using IndustryApp.Application.Abstractions;
using SkiaSharp;

namespace IndustryApp.Infrastructure.Imaging;

/// <summary>
/// Hattın taşıdığı görsel: çözülmüş pikseller, kaynaktaki meta veri bölümleri ve kalite. Decorator adımları bunları
/// değiştirir; JPEG kodlaması en sonda bir kez yapılır.
/// </summary>
internal sealed class SkiaProcessedImage(SKBitmap bitmap, IReadOnlyList<byte[]> metadataSegments) : IProcessedImage
{
    /// <summary>CompressionProcessor kaliteyi belirlemezse kullanılan değer.</summary>
    public const int DefaultQuality = 85;

    public SKBitmap Bitmap { get; private set; } = bitmap;

    /// <summary>Kaynaktan gelen EXIF/XMP/IPTC bölümleri; kodlamada çıktıya aynen yazılır.</summary>
    public IReadOnlyList<byte[]> MetadataSegments { get; set; } = metadataSegments;

    public int Width => Bitmap.Width;

    public int Height => Bitmap.Height;

    public int? Quality { get; set; }

    /// <summary>Pikselleri yenisiyle değiştirir; eskisini bırakır.</summary>
    public void Replace(SKBitmap next)
    {
        if (ReferenceEquals(next, Bitmap))
            return;
        Bitmap.Dispose();
        Bitmap = next;
    }

    public async Task EncodeJpegAsync(Stream destination, CancellationToken ct)
    {
        // JPEG saydamlık taşımaz: PNG/WebP'deki saydam alanlar siyah yerine beyaz olur.
        using var opaque = new SKBitmap(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Opaque));
        using (var canvas = new SKCanvas(opaque))
        {
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(Bitmap, 0, 0);
        }

        using var image = SKImage.FromBitmap(opaque);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, Quality ?? DefaultQuality)
            ?? throw new InvalidOperationException("JPEG kodlanamadı.");

        var bytes = JpegMetadataSegments.Insert(data.ToArray(), MetadataSegments);
        await destination.WriteAsync(bytes, ct);
    }

    public void Dispose() => Bitmap.Dispose();
}
