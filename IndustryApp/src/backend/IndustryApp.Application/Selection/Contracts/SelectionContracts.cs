using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Selection.Contracts;

/// <summary>Teklif seçimi isteği (endpoint 29). Web, mobil veya test aynı sözleşmeyi uygular (T-4.4).</summary>
public interface ISelectOfferRequest
{
    Guid ReportId { get; }

    Guid OfferId { get; }
}

public sealed record SelectOfferRequest(Guid ReportId, Guid OfferId) : ISelectOfferRequest;

/// <summary>Seçimi geri alma isteği (endpoint 30).</summary>
public interface IUndoSelectionRequest
{
    Guid ReportId { get; }
}

public sealed record UndoSelectionRequest(Guid ReportId) : IUndoSelectionRequest;

/// <summary>Seçim sonucu; UndoDeadline'a kadar müşteri "Vazgeç" diyebilir (FR-04.3).</summary>
public sealed record SelectionResponse(
    Guid ReportId,
    Guid OfferId,
    DateTimeOffset SelectedAt,
    DateTimeOffset UndoDeadline);

/// <summary>Raporun anlık seçim durumu (endpoint 31). Seçim yoksa SelectedOfferId ve tarihler null'dır.</summary>
public sealed record SelectionStatusResponse(
    Guid ReportId,
    ReportStatus Status,
    Guid? SelectedOfferId,
    DateTimeOffset? SelectedAt,
    DateTimeOffset? UndoDeadline,
    bool CanUndo);
