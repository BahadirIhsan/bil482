namespace IndustryApp.Application.Abstractions;

/// <summary>
/// Görsel işleme hattı (PAT-02.3): yükleme, boyutlandırma, sıkıştırma, EXIF/GPS temizleme. Okunamayan dosyada
/// UnsupportedImageException fırlatır.
/// </summary>
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

/// <summary>Depodaki işlenmiş fotoğrafın piksel boyutunu okur (rapora yazılan meta veri için).</summary>
public interface IImageInfoReader
{
    /// <summary>Görsel okunamazsa null.</summary>
    Task<(int Width, int Height)?> ReadSizeAsync(Stream content, CancellationToken ct);
}

/// <summary>Dosya imzası geçerli görünse de içerik çözülemedi (bozuk veya desteklenmeyen görsel).</summary>
public sealed class UnsupportedImageException(string message, Exception? inner = null) : Exception(message, inner);
