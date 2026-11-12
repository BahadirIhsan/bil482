using FluentAssertions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Reports.Expiration;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using IndustryApp.UnitTests.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Reports;

/// <summary>FR-02.4: teklif seçilmeyen rapor süre sonunda Expired olur (geçiş OpenState'te, PAT-04.1).</summary>
public sealed class ReportExpirationTests
{
    private readonly ServiceTestContext _ctx = new();
    private readonly ReportExpirationService _service;

    public ReportExpirationTests() =>
        _service = new ReportExpirationService(_ctx.Reports, _ctx.History, _ctx.UnitOfWork, _ctx.Clock,
            Options.Create(new ReportsOptions { ExpireAfterHours = 72, MaxOpenPerCustomer = 3, ExpirationBatchSize = 50 }),
            NullLogger<ReportExpirationService>.Instance);

    // TC-02.4.1
    [Fact]
    public async Task ExpirationJob_OpenReportPastTtl_BecomesExpired()
    {
        var report = DomainTestData.OpenReport(Guid.NewGuid());
        var pending = DomainTestData.OfferFor(report, Guid.NewGuid());
        _ctx.Now = report.ExpiresAt.AddMinutes(1);
        _ctx.Reports.ListExpiredOpenAsync(_ctx.Now, 50, Arg.Any<CancellationToken>()).Returns([report]);

        var count = await _service.ExpireDueAsync(CancellationToken.None);

        count.Should().Be(1);
        report.Status.Should().Be(ReportStatus.Expired);
        pending.Status.Should().Be(OfferStatus.Closed);
        report.DomainEvents.Should().ContainSingle(e => e is ReportExpiredEvent);
        _ctx.HistoryEntries.Should().ContainSingle(h =>
            h.CommandName == ReportCommandNames.ExpireReport && h.ActorUserId == null && h.ToStatus == ReportStatus.Expired);
        await _ctx.UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExpireDue_NothingDue_DoesNotSave()
    {
        _ctx.Reports.ListExpiredOpenAsync(Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);

        (await _service.ExpireDueAsync(CancellationToken.None)).Should().Be(0);
        await _ctx.UnitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
