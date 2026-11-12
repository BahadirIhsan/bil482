using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Infrastructure.Imaging;
using Microsoft.Extensions.Options;

namespace IndustryApp.UnitTests.Infrastructure;

/// <summary>PAT-02.3 Decorator: görsel hattı küçültür, sıkıştırır ve konum bilgisini siler (FR-02.1).</summary>
public sealed class ImagePipelineTests
{
    private static readonly IOptions<PhotosOptions> Options = Microsoft.Extensions.Options.Options.Create(
        new PhotosOptions { MaxWidthPx = 1920, JpegQuality = 80, MaxFileSizeMb = 10, MaxCountPerReport = 8, AllowedContentTypes = ["image/jpeg"] });

    /// <summary>DI'daki sırayla aynı hat.</summary>
    private static IImageProcessor FullPipeline() =>
        new ExifStripProcessor(new CompressionProcessor(new ResizeProcessor(new SkiaImageProcessor(), Options), Options));

    // TC-02.1.1
    [Fact]
    public async Task ImagePipeline_RemovesGpsExif()
    {
        var source = TestImages.JpegWithGpsExif(2400, 1200);
        TestImages.Contains(source, TestImages.ExifMarker).Should().BeTrue("kaynak dosya GPS etiketli olmalı");

        var output = await RunAsync(FullPipeline(), source);

        TestImages.Contains(output, TestImages.ExifMarker).Should().BeFalse();
        TestImages.SizeOf(output).Should().Be((1920, 960));
    }

    [Fact]
    public async Task ImagePipeline_WithoutStripStep_KeepsExif()
    {
        // Silme işini gerçekten ExifStripProcessor yapar: adım çıkarılınca meta veri dosyada kalır.
        var output = await RunAsync(new ResizeProcessor(new SkiaImageProcessor(), Options), TestImages.JpegWithGpsExif(800, 600));

        TestImages.Contains(output, TestImages.ExifMarker).Should().BeTrue();
    }

    [Fact]
    public async Task ImagePipeline_AppliesExifOrientationBeforeStripping()
    {
        // Orientation 6 (90° saat yönü): yatay kaydedilmiş dikey telefon fotoğrafı.
        var output = await RunAsync(FullPipeline(), TestImages.JpegWithGpsExif(400, 200, orientation: 6));

        TestImages.SizeOf(output).Should().Be((200, 400));
    }

    [Fact]
    public async Task ImagePipeline_SmallPng_IsConvertedToJpegWithoutResize()
    {
        var output = await RunAsync(FullPipeline(), TestImages.Png(640, 480));

        output.AsSpan(0, 3).ToArray().Should().Equal(0xFF, 0xD8, 0xFF);
        TestImages.SizeOf(output).Should().Be((640, 480));
    }

    [Fact]
    public async Task ImagePipeline_CorruptData_ThrowsUnsupportedImage()
    {
        var corrupt = TestImages.Jpeg(100, 100)[..20];

        var act = () => RunAsync(FullPipeline(), corrupt);

        await act.Should().ThrowAsync<UnsupportedImageException>();
    }

    private static async Task<byte[]> RunAsync(IImageProcessor pipeline, byte[] input)
    {
        using var processed = await pipeline.ProcessAsync(new Input(new MemoryStream(input)), CancellationToken.None);
        using var output = new MemoryStream();
        await processed.EncodeJpegAsync(output, CancellationToken.None);
        return output.ToArray();
    }

    private sealed record Input(Stream Content) : IImageInput
    {
        public string FileName => "test.jpg";

        public string ContentType => "image/jpeg";
    }
}
