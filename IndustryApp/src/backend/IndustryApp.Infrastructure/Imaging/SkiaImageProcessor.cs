using IndustryApp.Application.Abstractions;
using SkiaSharp;

namespace IndustryApp.Infrastructure.Imaging;

/// <summary>
/// Hattın çekirdeği (PAT-02.3'ün sarılan bileşeni): JPEG, PNG veya WebP dosyasını SkiaSharp ile çözer ve EXIF yönünü
/// piksellere uygular; böylece meta veri silindikten sonra fotoğraf yan dönmez. Çözülemeyen veya aşırı büyük
/// (piksel bombası) dosya UnsupportedImageException fırlatır.
/// </summary>
public sealed class SkiaImageProcessor : IImageProcessor
{
    /// <summary>Çözülecek en fazla piksel sayısı (≈ 50 MP); 10 MB'lık sıkıştırılmış dosya devasa bir tuval açamaz.</summary>
    public const long MaxPixels = 50_000_000;

    public async Task<IProcessedImage> ProcessAsync(IImageInput input, CancellationToken ct)
    {
        byte[] bytes;
        await using (var buffer = new MemoryStream())
        {
            await input.Content.CopyToAsync(buffer, ct);
            bytes = buffer.ToArray();
        }

        using var codec = SKCodec.Create(new SKMemoryStream(bytes))
            ?? throw new UnsupportedImageException("Görsel biçimi tanınmadı.");
        if (codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))
            throw new UnsupportedImageException($"Desteklenmeyen biçim: {codec.EncodedFormat}.");
        if ((long)codec.Info.Width * codec.Info.Height > MaxPixels)
            throw new UnsupportedImageException("Görsel çok büyük.");

        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var decoded = new SKBitmap(info);
        var result = codec.GetPixels(info, decoded.GetPixels());
        if (result is not (SKCodecResult.Success or SKCodecResult.IncompleteInput))
        {
            decoded.Dispose();
            throw new UnsupportedImageException($"Görsel çözülemedi: {result}.");
        }

        var oriented = ApplyOrigin(decoded, codec.EncodedOrigin);
        if (!ReferenceEquals(oriented, decoded))
            decoded.Dispose();

        var metadata = codec.EncodedFormat == SKEncodedImageFormat.Jpeg ? JpegMetadataSegments.Read(bytes) : [];
        return new SkiaProcessedImage(oriented, metadata);
    }

    /// <summary>EXIF Orientation değerine göre pikselleri döndürür/aynalar; dönüşüm gerekmiyorsa aynı bitmap döner.</summary>
    internal static SKBitmap ApplyOrigin(SKBitmap source, SKEncodedOrigin origin)
    {
        if (origin == SKEncodedOrigin.TopLeft)
            return source;

        var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var w = swap ? source.Height : source.Width;
        var h = swap ? source.Width : source.Height;

        // x' = scaleX·x + skewX·y + transX ; y' = skewY·x + scaleY·y + transY
        var matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, w, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, w, -1, 0, h, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, h, 0, 0, 1),
            _ => SKMatrix.Identity
        };

        var target = new SKBitmap(new SKImageInfo(w, h, source.ColorType, source.AlphaType));
        using var canvas = new SKCanvas(target);
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(source, 0, 0);
        return target;
    }
}
