namespace IndustryApp.Application.Offers.Commands;

/// <summary>report_status_history.command_name değerleri. Geri alma, en son SelectOffer kaydını bu adla arar.</summary>
public static class ReportCommandNames
{
    public const string SelectOffer = "SelectOfferCommand";
    public const string UndoSelectOffer = "UndoSelectOffer";
    public const string CompleteReport = "CompleteReport";
    public const string CancelReport = "CancelReport";
}
