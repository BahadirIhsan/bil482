namespace IndustryApp.Application.Abstractions;

/// <summary>Görsel işleme hattı (PAT-02.3): boyutlandırma, sıkıştırma, EXIF/GPS temizleme.</summary>
public interface IImageProcessor
{
    Task<IProcessedImage> ProcessAsync(IImageInput input, CancellationToken ct);
}

/// <summary>İşlenecek yüklenmiş dosya.</summary>
public interface IImageInput
{
    Stream Content { get; }

    string FileName { get; }

    string ContentType { get; }
}

/// <summary>İşlenmiş görsel. Kodlama (JPEG) çağıran tarafından yapılır; bellek kaynağı Dispose ile bırakılır.</summary>
public interface IProcessedImage : IDisposable
{
    int Width { get; }

    int Height { get; }

    /// <summary>Sıkıştırma adımının belirlediği JPEG kalitesi; belirlenmemişse null.</summary>
    int? Quality { get; }

    Task EncodeJpegAsync(Stream destination, CancellationToken ct);
}
