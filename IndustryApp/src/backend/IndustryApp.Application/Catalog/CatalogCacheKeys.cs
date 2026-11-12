namespace IndustryApp.Application.Catalog;

/// <summary>
/// Katalog önbellek anahtarları. Kategori ekleyen, adını değiştiren veya pasifleştiren servis (YA-01)
/// IMemoryCache.Remove(DamageTree) çağırır; sihirbaz değişikliği bir sonraki yüklemede görür.
/// </summary>
public static class CatalogCacheKeys
{
    public const string DamageTree = "catalog:damage-tree";
    public const string VehicleTypes = "catalog:vehicle-types";
}
