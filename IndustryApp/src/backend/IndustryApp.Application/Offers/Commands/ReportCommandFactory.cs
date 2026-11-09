using IndustryApp.Application.Common.Options;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using Microsoft.Extensions.Options;

namespace IndustryApp.Application.Offers.Commands;

/// <summary>
/// Rapor komutlarının tek üretim noktası (PAT-04.2). Geri alma süresi Offers:SelectionUndoWindowMinutes
/// ayarından okunur; koda gömülmez (T-0.3).
/// </summary>
public sealed class ReportCommandFactory(IOptions<OffersOptions> options) : IReportCommandFactory
{
    private TimeSpan UndoWindow => TimeSpan.FromMinutes(options.Value.SelectionUndoWindowMinutes);

    public IReportCommand CreateSelectOffer(AccidentReport report, Offer offer, Guid actorId, DateTimeOffset now) =>
        SelectOfferCommand.ForNewSelection(report, offer, actorId, now, UndoWindow);

    public IReportCommand Restore(
        AccidentReport report,
        ReportStatusHistory entry,
        IReadOnlyList<Offer> offers,
        Guid actorId,
        DateTimeOffset now)
    {
        if (entry.ReportId != report.Id)
            throw new InvalidOperationException("Geçmiş kaydı bu rapora ait değil.");

        return entry.CommandName switch
        {
            ReportCommandNames.SelectOffer => SelectOfferCommand.FromSnapshot(
                report,
                entry.ReadSnapshot() ?? throw new InvalidOperationException("Seçim kaydında snapshot yok."),
                offers,
                actorId,
                now,
                UndoWindow),
            _ => throw new InvalidOperationException($"'{entry.CommandName}' geri alınabilir bir komut değil.")
        };
    }
}
