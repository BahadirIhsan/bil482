using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Offers.Commands;

/// <summary>
/// Geri alınabilir rapor işlemi (PAT-04.2). İşlem bir nesne olarak paketlenir: Execute uygular, Undo tersine
/// çevirir, CreateSnapshot geri alma için gereken önceki durumu report_status_history'ye yazılmak üzere verir.
/// </summary>
public interface IReportCommand
{
    /// <summary>report_status_history.command_name kolonuna yazılan ad.</summary>
    string Name { get; }

    /// <summary>İşlemi yapan kullanıcı; geçmiş kaydının actor_user_id değeri.</summary>
    Guid ActorId { get; }

    void Execute();

    void Undo();

    ReportCommandSnapshot CreateSnapshot();
}
