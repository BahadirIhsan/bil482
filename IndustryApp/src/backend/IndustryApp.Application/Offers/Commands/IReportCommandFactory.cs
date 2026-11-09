using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Offers.Commands;

/// <summary>
/// Rapor komutlarını üretir (PAT-04.2). HTTP durumsuz olduğu için seçim isteğindeki komut nesnesi geri alma
/// isteğinde bellekte yoktur; Restore aynı komutu geçmiş kaydındaki snapshot'tan yeniden kurar (T-8.6).
/// </summary>
public interface IReportCommandFactory
{
    IReportCommand CreateSelectOffer(AccidentReport report, Offer offer, Guid actorId, DateTimeOffset now);

    IReportCommand Restore(
        AccidentReport report,
        ReportStatusHistory entry,
        IReadOnlyList<Offer> offers,
        Guid actorId,
        DateTimeOffset now);
}
