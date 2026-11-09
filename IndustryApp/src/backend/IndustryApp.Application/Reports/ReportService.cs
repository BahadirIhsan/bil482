using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using Microsoft.Extensions.Options;

namespace IndustryApp.Application.Reports;

/// <summary>
/// Rapor detayı, tamamlama ve iptal. Tamamlama ve iptal doğrudan raporun State nesnesine devredilir
/// (PAT-04.1); servis durum alanını hiçbir zaman kendisi değiştirmez.
/// </summary>
public sealed class ReportService(
    IAccidentReportRepository reports,
    IReportStatusHistoryRepository history,
    IVehicleTypeRepository vehicleTypes,
    IDamageCategoryRepository damageCategories,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IOptions<OffersOptions> offersOptions) : IReportService
{
    public async Task<Result<ReportDetailResponse>> GetForCustomerAsync(Guid reportId, CancellationToken ct)
    {
        var report = await reports.GetWithDetailsAsync(reportId, ct);
        if (report is null)
            return Errors.Reports.NotFound;
        if (!report.IsOwnedBy(currentUser.UserId))
            return Errors.Reports.NotOwner;

        var vehicleTypeName = (await vehicleTypes.ListActiveAsync(ct))
            .FirstOrDefault(v => v.Id == report.VehicleTypeId)?.Name ?? "-";

        var categoryIds = report.DamageItems.Select(d => d.DamageCategoryId).Distinct().ToList();
        var categoryNames = (await damageCategories.GetByIdsAsync(categoryIds, ct)).ToDictionary(c => c.Id, c => c.Name);

        var undoWindow = TimeSpan.FromMinutes(offersOptions.Value.SelectionUndoWindowMinutes);
        return new ReportDetailResponse(
            report.Id,
            report.Status,
            report.City,
            report.District,
            report.AddressText,
            report.Description,
            vehicleTypeName,
            report.CreatedAt,
            report.ExpiresAt,
            report.SelectedOfferId,
            report.OfferSelectedAt,
            report.UndoDeadline(undoWindow),
            report.CompletedAt,
            report.CancelledAt,
            report.DamageItems
                .Select(d => new ReportDamageItemResponse(
                    d.DamageCategoryId, categoryNames.GetValueOrDefault(d.DamageCategoryId, "-"), d.Severity, d.Note))
                .ToList(),
            report.Photos.OrderBy(p => p.SortOrder).Select(p => p.Id).ToList());
    }

    public async Task<Result> CompleteAsync(Guid reportId, CancellationToken ct)
    {
        var report = await reports.GetWithOffersAsync(reportId, ct);
        if (report is null)
            return Errors.Reports.NotFound;
        if (!report.IsOwnedBy(currentUser.UserId))
            return Errors.Reports.NotOwner;

        var now = clock.UtcNow;
        var fromStatus = report.Status;
        report.MarkCompleted(now); // yalnızca OfferSelected kabul eder

        await history.AddAsync(
            ReportStatusHistory.Create(report.Id, fromStatus, report.Status, ReportCommandNames.CompleteReport, currentUser.UserId, null, now),
            ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> CancelAsync(Guid reportId, CancellationToken ct)
    {
        var report = await reports.GetWithOffersAsync(reportId, ct);
        if (report is null)
            return Errors.Reports.NotFound;
        if (!report.IsOwnedBy(currentUser.UserId))
            return Errors.Reports.NotOwner;

        var now = clock.UtcNow;
        var fromStatus = report.Status;
        report.Cancel(now); // Open veya OfferSelected kabul eder

        // Hâlâ karar bekleyen veya kabul edilmiş teklifler kapanır; NotSelected ve Withdrawn olduğu gibi kalır.
        foreach (var offer in report.Offers.Where(o => o.Status is OfferStatus.Pending or OfferStatus.Accepted))
            offer.Close(now);

        await history.AddAsync(
            ReportStatusHistory.Create(report.Id, fromStatus, report.Status, ReportCommandNames.CancelReport, currentUser.UserId, null, now),
            ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
