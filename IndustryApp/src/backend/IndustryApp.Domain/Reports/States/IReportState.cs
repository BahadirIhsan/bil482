using IndustryApp.Domain.Offers;

namespace IndustryApp.Domain.Reports.States;

/// <summary>Rapor üzerindeki her işlemi bildiren durum arayüzü (PAT-04.1).</summary>
public interface IReportState
{
    ReportStatus Status { get; }

    bool CanReceiveOffers { get; }

    void SelectOffer(AccidentReport report, Offer offer, DateTimeOffset now);

    void UndoSelection(AccidentReport report, DateTimeOffset now, TimeSpan window);

    void Complete(AccidentReport report, DateTimeOffset now);

    void Cancel(AccidentReport report, DateTimeOffset now);

    void Expire(AccidentReport report, DateTimeOffset now);
}
