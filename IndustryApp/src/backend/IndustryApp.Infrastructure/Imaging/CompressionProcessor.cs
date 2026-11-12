using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace IndustryApp.Infrastructure.Imaging;

/// <summary>JPEG kalitesini Photos:JpegQuality olarak belirler; kodlama hattın sonunda bu değerle yapılır.</summary>
public sealed class CompressionProcessor(IImageProcessor inner, IOptions<PhotosOptions> options) : ImageProcessorDecorator(inner)
{
    internal override void Apply(SkiaProcessedImage image) => image.Quality = options.Value.JpegQuality;
}
