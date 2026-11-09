using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Domain.Reports;
using IndustryApp.UnitTests.Domain;
using NSubstitute;

namespace IndustryApp.UnitTests.Application;

/// <summary>
/// Application servis testleri için sahte bağımlılıklar (PAT-X.1: servisler veritabanısız test edilir).
/// Saat sabittir; geçmiş kayıtları bellekte tutulur ve GetLatestAsync onları döndürür.
/// </summary>
internal sealed class ServiceTestContext
{
    public ServiceTestContext()
    {
        Clock.UtcNow.Returns(_ => Now);
        CurrentUser.UserId.Returns(_ => UserId);
        CurrentUser.IsAuthenticated.Returns(true);

        History.When(h => h.AddAsync(Arg.Any<ReportStatusHistory>(), Arg.Any<CancellationToken>()))
            .Do(call => HistoryEntries.Add(call.Arg<ReportStatusHistory>()));
        History.GetLatestAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => HistoryEntries
                .Where(h => h.ReportId == call.ArgAt<Guid>(0) && h.CommandName == call.ArgAt<string>(1))
                .LastOrDefault());
    }

    public DateTimeOffset Now { get; set; } = DomainTestData.Now.AddHours(1);

    public Guid UserId { get; set; } = Guid.NewGuid();

    public IAccidentReportRepository Reports { get; } = Substitute.For<IAccidentReportRepository>();

    public IReportStatusHistoryRepository History { get; } = Substitute.For<IReportStatusHistoryRepository>();

    public List<ReportStatusHistory> HistoryEntries { get; } = new();

    public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();

    public ICurrentUser CurrentUser { get; } = Substitute.For<ICurrentUser>();

    public IDateTimeProvider Clock { get; } = Substitute.For<IDateTimeProvider>();

    public void Returns(AccidentReport report)
    {
        Reports.GetWithOffersAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);
        Reports.GetWithDetailsAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);
    }
}
