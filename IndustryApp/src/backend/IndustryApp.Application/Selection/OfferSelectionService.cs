using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Selection.Contracts;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using Microsoft.Extensions.Options;

namespace IndustryApp.Application.Selection;

/// <summary>
/// UC-04 seçim akışının istemcisi (Command deseninde invoker). Sahiplik kontrolü burada, durum kararı
/// State'te (PAT-04.1), değişikliğin kendisi SelectOfferCommand'dadır (PAT-04.2). Rapor, teklifler ve geçmiş
/// tek transaction'da kaydedilir (PAT-X.1); bildirimler commit sonrası olaylarla gider (PAT-03.1).
/// </summary>
public sealed class OfferSelectionService(
    IAccidentReportRepository reports,
    IReportStatusHistoryRepository history,
    IReportCommandFactory commands,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IOptions<OffersOptions> options) : IOfferSelectionService
{
    private TimeSpan UndoWindow => TimeSpan.FromMinutes(options.Value.SelectionUndoWindowMinutes);

    public async Task<Result<SelectionResponse>> SelectAsync(ISelectOfferRequest request, CancellationToken ct)
    {
        if (request.OfferId == Guid.Empty)
            return Errors.Offers.OfferIdRequired;

        var report = await reports.GetWithOffersAsync(request.ReportId, ct);
        if (report is null)
            return Errors.Reports.NotFound;
        if (!report.IsOwnedBy(currentUser.UserId))
            return Errors.Reports.NotOwner;

        var offer = report.Offers.FirstOrDefault(o => o.Id == request.OfferId);
        if (offer is null)
            return Errors.Offers.NotFound;
        if (offer.Status != OfferStatus.Pending)
            return Errors.Offers.NotAvailable;

        var now = clock.UtcNow;
        var fromStatus = report.Status;

        var command = commands.CreateSelectOffer(report, offer, currentUser.UserId, now);
        command.Execute(); // Open değilse State REPORT_INVALID_STATE fırlatır → 409

        await history.AddAsync(
            ReportStatusHistory.Create(report.Id, fromStatus, report.Status, command.Name, command.ActorId, command.CreateSnapshot(), now),
            ct);

        // Aynı anda gelen ikinci seçim xmin veya ux_offers_one_accepted_per_report ile ConflictException alır → 409.
        await unitOfWork.SaveChangesAsync(ct);

        return new SelectionResponse(report.Id, offer.Id, now, now + UndoWindow);
    }

    public async Task<Result<SelectionStatusResponse>> UndoAsync(IUndoSelectionRequest request, CancellationToken ct)
    {
        var report = await reports.GetWithOffersAsync(request.ReportId, ct);
        if (report is null)
            return Errors.Reports.NotFound;
        if (!report.IsOwnedBy(currentUser.UserId))
            return Errors.Reports.NotOwner;
        if (report.Status != ReportStatus.OfferSelected)
            return Errors.Selection.NothingToUndo;

        var entry = await history.GetLatestAsync(report.Id, ReportCommandNames.SelectOffer, ct);
        if (entry is null)
            return Errors.Selection.NothingToUndo;

        var now = clock.UtcNow;
        var command = commands.Restore(report, entry, report.Offers.ToList(), currentUser.UserId, now);
        command.Undo(); // süre dışıysa State UNDO_WINDOW_EXPIRED fırlatır → 409

        await history.AddAsync(
            ReportStatusHistory.Create(
                report.Id, ReportStatus.OfferSelected, report.Status, ReportCommandNames.UndoSelectOffer, command.ActorId, null, now),
            ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ToStatus(report, now, isOwner: true);
    }

    public async Task<Result<SelectionStatusResponse>> GetAsync(Guid reportId, CancellationToken ct)
    {
        var report = await reports.GetWithOffersAsync(reportId, ct);
        if (report is null)
            return Errors.Reports.NotFound;

        var viewerId = currentUser.UserId;
        var isOwner = report.IsOwnedBy(viewerId);
        var isMechanicParty = report.Matches.Any(m => m.MechanicId == viewerId) || report.Offers.Any(o => o.MechanicId == viewerId);
        if (!isOwner && !isMechanicParty)
            return Errors.Offers.NotMatched;

        return ToStatus(report, clock.UtcNow, isOwner);
    }

    private SelectionStatusResponse ToStatus(AccidentReport report, DateTimeOffset now, bool isOwner)
    {
        var deadline = report.UndoDeadline(UndoWindow);
        var canUndo = isOwner && report.Status == ReportStatus.OfferSelected && deadline is not null && now <= deadline;
        return new SelectionStatusResponse(report.Id, report.Status, report.SelectedOfferId, report.OfferSelectedAt, deadline, canUndo);
    }
}
