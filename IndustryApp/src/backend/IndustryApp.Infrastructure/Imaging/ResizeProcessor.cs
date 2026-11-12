using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Options;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace IndustryApp.Infrastructure.Imaging;

/// <summary>Genişliği Photos:MaxWidthPx'i aşan görseli oranı koruyarak küçültür; küçük görsele dokunmaz.</summary>
public sealed class ResizeProcessor(IImageProcessor inner, IOptions<PhotosOptions> options) : ImageProcessorDecorator(inner)
{
    internal override void Apply(SkiaProcessedImage image)
    {
        var maxWidth = options.Value.MaxWidthPx;
        if (image.Width <= maxWidth)
            return;

        var height = Math.Max(1, (int)Math.Round(image.Height * (double)maxWidth / image.Width));
        var info = new SKImageInfo(maxWidth, height, image.Bitmap.ColorType, image.Bitmap.AlphaType);
        var resized = image.Bitmap.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell))
            ?? throw new InvalidOperationException("Görsel boyutlandırılamadı.");
        image.Replace(resized);
    }
}
