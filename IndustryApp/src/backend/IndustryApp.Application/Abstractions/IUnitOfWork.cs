namespace IndustryApp.Application.Abstractions;

// [ALL][PAT-X.1] Repository + Unit of Work
// Niyet   : Use case'leri EF Core'dan ayırmak ve bir use case'in birden çok aggregate'ini tek transaction'da kaydetmek.
// Neden   : Servisler DbContext'e bağlı olsaydı veritabanısız test edilemezdi; UC-04'te rapor, teklifler ve
//           geçmiş birlikte yazılmalıdır.
// Kazanım : Kalıcılık teknolojisi servislere dokunmadan değişebilir; olaylar yalnızca commit başarılıysa yayınlanır.
// Metrik  : Birim testlerinin %100'ü veritabanısız çalışır.
public interface IUnitOfWork
{
    /// <summary>Kaydeder; başarılıysa biriken domain olaylarını IEventDispatcher'a iletir.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct);
}
