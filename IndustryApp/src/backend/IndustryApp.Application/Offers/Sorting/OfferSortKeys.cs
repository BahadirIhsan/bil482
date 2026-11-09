namespace IndustryApp.Application.Offers.Sorting;

/// <summary>GET /api/reports/{id}/offers?sort= değerleri. Frontend'deki OfferSortKey ile aynıdır.</summary>
public static class OfferSortKeys
{
    public const string Price = "price";
    public const string Fastest = "fastest";
    public const string Newest = "newest";

    /// <summary>Anahtar boş veya tanımsızsa kullanılan sıralama (FR-04.1).</summary>
    public const string Default = Price;
}
