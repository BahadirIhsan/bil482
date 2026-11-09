namespace IndustryApp.Application.Offers.Contracts;

/// <summary>Müşterinin rapor teklifleri sorgusu (endpoint 27). SortKey boşsa fiyata göre sıralanır.</summary>
public interface IReportOffersQuery
{
    Guid ReportId { get; }

    string? SortKey { get; }
}

public sealed record ReportOffersQuery(Guid ReportId, string? SortKey) : IReportOffersQuery;
