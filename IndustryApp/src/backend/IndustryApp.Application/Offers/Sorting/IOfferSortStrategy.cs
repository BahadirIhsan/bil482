using IndustryApp.Domain.Offers;

namespace IndustryApp.Application.Offers.Sorting;

// [UC-04][PAT-04.4] Strategy (sıralama)
// Niyet   : Teklif listesinin sıralama ölçütünü (fiyat, en kısa süre, en yeni) değiştirilebilir nesnelere ayırmak.
// Neden   : Müşteri teklifleri farklı ölçütlerle karşılaştırır ve yenileri (ör. puan) gelecektir. Ölçüt controller'da
//           veya serviste switch ile seçilseydi her yeni sıralama mevcut kodu değiştirirdi; bellekte sıralama ise
//           tüm satırları yüklerdi.
// Kazanım : Her strateji IQueryable'a ORDER BY ekler, sıralama SQL'de çalışır; OfferService anahtara uyan stratejiyi
//           DI'dan seçer, bilinmeyen anahtar fiyata düşer.
// Metrik  : Yeni sıralama = 1 yeni sınıf, 0 değişen sınıf (PriceAscendingSort_OrdersByTotalPrice TC-04.1.2,
//           FastestCompletionSort_OrdersByDaysThenPrice TC-04.1.3, NewestFirstSort_OrdersByCreatedAtDescending TC-04.1.4).
/// <summary>Teklif sıralama stratejisi. Tüm uygulamaları DI'a IOfferSortStrategy olarak kayıtlıdır.</summary>
public interface IOfferSortStrategy
{
    /// <summary>İstekteki sıralama anahtarı: price, fastest, newest.</summary>
    string Key { get; }

    IQueryable<Offer> Apply(IQueryable<Offer> offers);
}
