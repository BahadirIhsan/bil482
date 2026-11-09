using IndustryApp.Application.Common;
using IndustryApp.Application.Selection.Contracts;

namespace IndustryApp.Application.Selection;

/// <summary>UC-04 teklif seçimi, geri alma ve seçim durumu (FR-04.2, FR-04.3).</summary>
public interface IOfferSelectionService
{
    Task<Result<SelectionResponse>> SelectAsync(ISelectOfferRequest request, CancellationToken ct);

    Task<Result<SelectionStatusResponse>> UndoAsync(IUndoSelectionRequest request, CancellationToken ct);

    Task<Result<SelectionStatusResponse>> GetAsync(Guid reportId, CancellationToken ct);
}
