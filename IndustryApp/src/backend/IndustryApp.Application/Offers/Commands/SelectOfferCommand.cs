using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Offers.Commands;

// [UC-04][PAT-04.2] Command
// Niyet   : Teklif seçimini yürütülebilir ve geri alınabilir tek bir nesne olarak paketlemek.
// Neden   : Seçim aynı anda raporu ve birden fazla teklifi değiştirir ve 10 dakika içinde birebir geri
//           alınabilmelidir. Alternatif olan ayrı bir "geri alma servisi", seçim mantığını tersten ikinci
//           kez yazardı ve iki kopya zamanla birbirinden ayrışırdı.
// Kazanım : Execute ve Undo simetrik olarak tek sınıfta durur; CreateSnapshot sayesinde komut veritabanına
//           yazılıp sonraki istekte yeniden kurulabilir ve her seçim bir denetim kaydı bırakır.
// Metrik  : Undo sonrası durum = seçimden önceki durum; süre dışında geri alma %100 reddedilir
//           (Undo_WithinWindow_RestoresPendingOffers TC-04.3.1, Undo_AfterWindow_Throws TC-04.3.2).
/// <summary>
/// Müşterinin bir teklifi seçmesi. Durum kararını raporun State nesnesi verir (PAT-04.1): yalnızca Open rapor
/// seçim kabul eder, yalnızca süre içindeki OfferSelected rapor geri alınabilir.
/// </summary>
public sealed class SelectOfferCommand : IReportCommand
{
    private readonly AccidentReport _report;
    private readonly Offer _chosen;
    private readonly IReadOnlyList<Offer> _others;
    private readonly DateTimeOffset _now;
    private readonly TimeSpan _undoWindow;

    private SelectOfferCommand(
        AccidentReport report,
        Offer chosen,
        IReadOnlyList<Offer> others,
        Guid actorId,
        DateTimeOffset now,
        TimeSpan undoWindow)
    {
        _report = report;
        _chosen = chosen;
        _others = others;
        ActorId = actorId;
        _now = now;
        _undoWindow = undoWindow;
    }

    public string Name => ReportCommandNames.SelectOffer;

    public Guid ActorId { get; }

    /// <summary>Yeni seçim: seçilen dışındaki Pending teklifler "seçilmedi" olacak teklifler olarak alınır.</summary>
    internal static SelectOfferCommand ForNewSelection(
        AccidentReport report, Offer chosen, Guid actorId, DateTimeOffset now, TimeSpan undoWindow)
    {
        var others = report.Offers
            .Where(o => o.Id != chosen.Id && o.Status == OfferStatus.Pending)
            .ToList();
        return new SelectOfferCommand(report, chosen, others, actorId, now, undoWindow);
    }

    /// <summary>Geri alma için yeniden kurulum: etkilenen teklifler snapshot'taki kimliklerden gelir.</summary>
    internal static SelectOfferCommand FromSnapshot(
        AccidentReport report,
        ReportCommandSnapshot snapshot,
        IReadOnlyList<Offer> offers,
        Guid actorId,
        DateTimeOffset now,
        TimeSpan undoWindow)
    {
        var chosen = offers.SingleOrDefault(o => o.Id == snapshot.SelectedOfferId)
            ?? throw new InvalidOperationException("Snapshot'taki seçilen teklif raporun teklifleri arasında yok.");
        var otherIds = snapshot.NotSelectedOfferIds.ToHashSet();
        var others = offers.Where(o => otherIds.Contains(o.Id)).ToList();
        if (others.Count != otherIds.Count)
            throw new InvalidOperationException("Snapshot'taki tekliflerin bir kısmı yüklenemedi.");

        return new SelectOfferCommand(report, chosen, others, actorId, now, undoWindow);
    }

    public void Execute()
    {
        _report.SelectOffer(_chosen, _now); // State karar verir: yalnızca Open kabul eder
        _chosen.Accept(_now);
        foreach (var offer in _others)
            offer.MarkNotSelected(_now);
    }

    public void Undo()
    {
        _report.UndoSelection(_now, _undoWindow); // State: süre dışıysa UNDO_WINDOW_EXPIRED
        _chosen.Reopen(_now);
        foreach (var offer in _others)
            offer.Reopen(_now);
    }

    public ReportCommandSnapshot CreateSnapshot() =>
        new(_chosen.Id, _others.Select(o => o.Id).ToArray());
}
