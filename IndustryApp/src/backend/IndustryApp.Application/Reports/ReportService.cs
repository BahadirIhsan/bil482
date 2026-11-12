using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Application.Reports.Photos;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IndustryApp.Application.Reports;

/// <summary>
/// Rapor oluşturma ve listeleme (UC-02), detay, tamamlama ve iptal (UC-04). Rapor yalnızca AccidentReportBuilder
/// ile kurulur (PAT-02.1); tamamlama ve iptal raporun State nesnesine devredilir (PAT-04.1). Eşleştirmeyi başlatmaz:
/// commit sonrası AccidentReportCreatedEvent kuyruğa girer ve UC-03 arka planda çalışır (PAT-03.1).
/// </summary>
public sealed class ReportService(
    IAccidentReportRepository reports,
    IReportStatusHistoryRepository history,
    IVehicleTypeRepository vehicleTypes,
    IDamageCategoryRepository damageCategories,
    IFileStorage storage,
    IImageInfoReader imageInfo,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IOptions<OffersOptions> offersOptions,
    IOptions<ReportsOptions> reportsOptions,
    ILogger<ReportService> log) : IReportService
{
    private const string JpegContentType = "image/jpeg";
    private const int MaxPageSize = 100;

    public async Task<Result<CreateReportResponse>> CreateAsync(ICreateReportRequest request, CancellationToken ct)
    {
        var customerId = currentUser.UserId;
        var options = reportsOptions.Value;

        if (await reports.CountOpenByCustomerAsync(customerId, ct) >= options.MaxOpenPerCustomer)
            return Errors.Reports.OpenLimitReached;
        if (request.VehicleTypeId > 0 && !await vehicleTypes.ExistsAsync(request.VehicleTypeId, ct))
            return Errors.Reports.VehicleTypeInvalid;

        // UC-02 7b: anahtar yalnızca bu müşterinin geçici klasöründen olabilir; başka yol (../, reports/...) kabul edilmez.
        var photoKeys = request.PhotoKeys ?? [];
        if (photoKeys.Any(k => !PhotoKeys.IsTemporaryOf(k, customerId)))
            return Errors.Reports.InvalidPhotoKey;

        var damages = request.Damages ?? [];
        var categoryIds = damages.Select(d => d.CategoryId).Distinct().ToList();
        var categories = (await damageCategories.GetByIdsAsync(categoryIds, ct)).ToDictionary(c => c.Id);
        if (categories.Count != categoryIds.Count)
            return Errors.Reports.CategoryInactive;

        var now = clock.UtcNow;
        var builder = new AccidentReportBuilder().WithCustomer(customerId).WithDescription(request.Description);
        var moves = new List<(string From, string To)>();
        AccidentReport report;
        try
        {
            if (request.VehicleTypeId > 0)
                builder.WithVehicleType(request.VehicleTypeId);
            if (!string.IsNullOrWhiteSpace(request.City) && request.Latitude is { } lat && request.Longitude is { } lng)
                builder.AtLocation(request.City, request.District, lat, lng, request.AddressText);
            foreach (var damage in damages)
                builder.AddDamage(categories[damage.CategoryId], damage.Severity, damage.Note);

            for (var i = 0; i < photoKeys.Count; i++)
            {
                var meta = await ReadTemporaryPhotoAsync(photoKeys[i], ct);
                if (meta is null)
                    return Errors.Reports.InvalidPhotoKey;

                var photoId = Guid.NewGuid();
                var finalKey = PhotoKeys.Final(now, builder.ReportId, photoId);
                builder.AddPhoto(photoId, finalKey, meta, i);
                moves.Add((photoKeys[i], finalKey));
            }

            report = builder.Build(now, TimeSpan.FromHours(options.ExpireAfterHours));
        }
        catch (DomainException ex)
        {
            // REPORT_INCOMPLETE, CATEGORY_INACTIVE, DUPLICATE_DAMAGE_ITEM, INVALID_LOCATION: istek hatasıdır (400).
            return Error.Validation(ex.Code, ex.Message);
        }

        // Dosyalar kayıttan önce kalıcı anahtarlarına taşınır; kayıt başarısız olursa geri taşınır (SDD 9, telafi).
        var moved = new List<(string From, string To)>();
        try
        {
            foreach (var move in moves)
            {
                await storage.MoveAsync(move.From, move.To, ct);
                moved.Add(move);
            }

            await reports.AddAsync(report, ct);
            await history.AddAsync(
                ReportStatusHistory.Create(report.Id, null, report.Status, ReportCommandNames.CreateReport, customerId, null, now), ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (FileNotFoundException)
        {
            await CompensateAsync(moved);
            return Errors.Reports.InvalidPhotoKey;
        }
        catch
        {
            await CompensateAsync(moved);
            throw;
        }

        log.LogInformation("Rapor oluşturuldu {ReportId}: {PhotoCount} fotoğraf, {DamageCount} hasar kalemi",
            report.Id, moves.Count, damages.Count);
        return new CreateReportResponse(report.Id, report.Status, report.ExpiresAt);
    }

    public async Task<Result<PagedResult<ReportListItemResponse>>> ListMineAsync(IPagedQuery query, CancellationToken ct)
    {
        var paging = new Paging(Math.Max(1, query.Page), Math.Clamp(query.PageSize, 1, MaxPageSize));
        return await reports.ListByCustomerAsync(currentUser.UserId, paging, ct);
    }

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

    /// <summary>Geçici fotoğrafın boyutu ve piksel ölçüsü; dosya yoksa veya okunamazsa null.</summary>
    private async Task<PhotoMetadata?> ReadTemporaryPhotoAsync(string key, CancellationToken ct)
    {
        var size = await storage.GetSizeAsync(key, ct);
        if (size is null or <= 0 or > int.MaxValue)
            return null;

        await using var stream = await storage.OpenReadAsync(key, ct);
        if (stream is null)
            return null;

        var dimensions = await imageInfo.ReadSizeAsync(stream, ct);
        return dimensions is { } d ? new PhotoMetadata(JpegContentType, (int)size.Value, d.Width, d.Height) : null;
    }

    private async Task CompensateAsync(IEnumerable<(string From, string To)> moved)
    {
        foreach (var (from, to) in moved)
        {
            try
            {
                await storage.MoveAsync(to, from, CancellationToken.None);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Telafi: {Key} geri taşınamadı, siliniyor", to);
                await storage.DeleteAsync(to, CancellationToken.None);
            }
        }
    }

    private sealed record Paging(int Page, int PageSize) : IPagedQuery;
}
