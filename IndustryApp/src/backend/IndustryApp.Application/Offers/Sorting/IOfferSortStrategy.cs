using IndustryApp.Domain.Offers;

namespace IndustryApp.Application.Offers.Sorting;

/// <summary>
/// Teklif sıralama stratejisi (PAT-04.4). Somut stratejiler (fiyat, en yeni, en hızlı) Faz 2'de eklenir;
/// arayüz, sıralı listeyi döndüren repository imzası için şimdiden tanımlıdır.
/// </summary>
public interface IOfferSortStrategy
{
    /// <summary>İstekteki sıralama anahtarı: price, newest, fastest.</summary>
    string Key { get; }

    IQueryable<Offer> Apply(IQueryable<Offer> offers);
}
