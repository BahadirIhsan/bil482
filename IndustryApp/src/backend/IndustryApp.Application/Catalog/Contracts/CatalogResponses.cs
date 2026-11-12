namespace IndustryApp.Application.Catalog.Contracts;

/// <summary>Hasar ağacı düğümü (endpoint 11). Usta kaydında 1. seviye düğümler uzmanlık olarak seçilir.</summary>
public sealed record DamageCategoryNodeResponse(
    int Id,
    string Code,
    string Name,
    short Level,
    IReadOnlyList<DamageCategoryNodeResponse> Children);

/// <summary>Kayıt formunda onaylanacak güncel sözleşme (endpoint 13).</summary>
public sealed record LegalDocumentResponse(int Id, string Code, string Version, string Title, string Body);

/// <summary>Rapor sihirbazındaki araç türü seçeneği (endpoint 12).</summary>
public sealed record VehicleTypeResponse(short Id, string Code, string Name);
