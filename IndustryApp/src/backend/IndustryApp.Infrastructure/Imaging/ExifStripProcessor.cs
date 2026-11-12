using IndustryApp.Application.Abstractions;

namespace IndustryApp.Infrastructure.Imaging;

/// <summary>
/// Konum (GPS) dahil EXIF, XMP ve IPTC meta verisini siler (FR-02.1, KVKK). Bu adım hatta yoksa kaynak JPEG'in
/// meta veri bölümleri saklanan dosyaya aynen yazılır.
/// </summary>
public sealed class ExifStripProcessor(IImageProcessor inner) : ImageProcessorDecorator(inner)
{
    internal override void Apply(SkiaProcessedImage image) => image.MetadataSegments = [];
}
