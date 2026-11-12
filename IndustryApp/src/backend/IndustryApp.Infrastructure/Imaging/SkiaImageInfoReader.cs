using IndustryApp.Application.Abstractions;
using SkiaSharp;

namespace IndustryApp.Infrastructure.Imaging;

/// <summary>Görselin yalnızca başlığını okuyarak piksel boyutunu döner; pikseller çözülmez.</summary>
public sealed class SkiaImageInfoReader : IImageInfoReader
{
    public async Task<(int Width, int Height)?> ReadSizeAsync(Stream content, CancellationToken ct)
    {
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        using var codec = SKCodec.Create(buffer);
        return codec is null ? null : (codec.Info.Width, codec.Info.Height);
    }
}
