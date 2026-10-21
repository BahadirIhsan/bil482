using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Offers.Contracts;

/// <summary>Ustanın "Tekliflerim" listesindeki satır (UC-03).</summary>
public sealed record MyOfferListItemResponse(
    Guid Id,
    Guid ReportId,
    string City,
    string? District,
    decimal TotalPrice,
    string Currency,
    short EstimatedDays,
    OfferStatus Status,
    ReportStatus ReportStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
