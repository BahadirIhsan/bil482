using IndustryApp.Application.Abstractions;

namespace IndustryApp.Infrastructure.Imaging;

// [UC-02][PAT-02.3] Decorator
// Niyet   : Her fotoğrafa boyutlandırma, sıkıştırma ve EXIF/GPS temizleme adımlarını birbirinden bağımsız eklemek.
// Neden   : Tüm adımlar tek sınıfta olsaydı yeni adım (ör. filigran) mevcut kodu değiştirir, adımlar ayrı test edilemezdi.
// Kazanım : Her adım bir sınıftır; sıra ve adım seti DI'da kurulur; adımlar tek tek test edilir.
// Metrik  : Saklanan fotoğraflarda GPS etiketi: 0 (ImagePipeline_RemovesGpsExif, TC-02.1.1).
/// <summary>Hat adımlarının tabanı: önce içteki işlemciyi çalıştırır, sonra kendi adımını uygular.</summary>
public abstract class ImageProcessorDecorator(IImageProcessor inner) : IImageProcessor
{
    public async Task<IProcessedImage> ProcessAsync(IImageInput input, CancellationToken ct)
    {
        var processed = await inner.ProcessAsync(input, ct);
        if (processed is not SkiaProcessedImage image)
            throw new InvalidOperationException("Hat SkiaImageProcessor ile başlamalıdır.");

        try
        {
            Apply(image);
            return image;
        }
        catch
        {
            image.Dispose();
            throw;
        }
    }

    internal abstract void Apply(SkiaProcessedImage image);
}
