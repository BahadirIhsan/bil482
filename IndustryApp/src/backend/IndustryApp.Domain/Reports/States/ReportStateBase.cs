using IndustryApp.Domain.Common;
using IndustryApp.Domain.Offers;

namespace IndustryApp.Domain.Reports.States;

// [UC-04][PAT-04.1] State
// Niyet   : Rapora hangi işlemin yapılabileceğini o anki durum nesnesine karar verdirmek.
// Neden   : Open, OfferSelected, Completed, Cancelled, Expired durum kontrolleri servislere dağılırsa
//           biri kolayca unutulur; alternatif olan if/switch blokları her yeni durumda her yerde değişirdi.
// Kazanım : İzin verilen geçişler sınıf başına açık ve test edilebilir; yeni durum yeni bir sınıftır.
// Metrik  : Geçersiz geçişlerin %100'ü reddedilir
//           (OpenState_SelectOffer_MovesToOfferSelected TC-04.2.1, CompletedState_SelectOffer_Throws TC-04.2.2).
/// <summary>Taban durum: tüm işlemler varsayılan olarak reddedilir; durumlar yalnızca izin verdiklerini ezer.</summary>
public abstract class ReportStateBase : IReportState
{
    public abstract ReportStatus Status { get; }

    public virtual bool CanReceiveOffers => false;

    public virtual void SelectOffer(AccidentReport report, Offer offer, DateTimeOffset now) => Deny(nameof(SelectOffer));

    public virtual void UndoSelection(AccidentReport report, DateTimeOffset now, TimeSpan window) => Deny(nameof(UndoSelection));

    public virtual void Complete(AccidentReport report, DateTimeOffset now) => Deny(nameof(Complete));

    public virtual void Cancel(AccidentReport report, DateTimeOffset now) => Deny(nameof(Cancel));

    public virtual void Expire(AccidentReport report, DateTimeOffset now) => Deny(nameof(Expire));

    protected void Deny(string operation) =>
        throw new DomainException(ErrorCodes.ReportInvalidState, $"'{Status}' durumunda '{operation}' yapılamaz.");
}
