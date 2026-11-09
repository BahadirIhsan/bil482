namespace IndustryApp.Application.Contacts.Contracts;

/// <summary>Karşı tarafın rolü: müşteri bakıyorsa usta, usta bakıyorsa müşteri.</summary>
public enum ContactCounterpartRole
{
    Customer,
    Mechanic
}

/// <summary>
/// Eşleşme ekranındaki iletişim bilgisi (endpoint 32). Değerler her zaman ContactInfoProxy'den gelir:
/// IsRevealed=false ise telefon maskeli, e-posta "-" ve adres null'dır (FR-04.4).
/// </summary>
public sealed record MatchContactResponse(
    Guid ReportId,
    ContactCounterpartRole CounterpartRole,
    string FullName,
    string Phone,
    string Email,
    bool IsRevealed,
    string? WorkshopName,
    string? Address);
