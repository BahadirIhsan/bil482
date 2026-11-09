using FluentAssertions;
using IndustryApp.Application.Common;
using IndustryApp.Application.Common.Exceptions;
using IndustryApp.Application.Contacts;
using IndustryApp.Application.Selection;
using IndustryApp.Application.Selection.Contracts;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using IndustryApp.Infrastructure.Persistence;
using IndustryApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryApp.IntegrationTests.Uc04;

/// <summary>UC-04 gerçek PostgreSQL üzerinde: eşzamanlılık koruması, kayıt bütünlüğü ve iletişim maskeleme.</summary>
[Collection(PostgresCollection.Name)]
public sealed class OfferSelectionIntegrationTests(PostgresFixture fixture)
{
    // TC-04.2.3
    [Fact]
    public async Task ConcurrentSelect_SecondRequestGets409()
    {
        var s = await Uc04TestData.CreateAsync(fixture);

        // İki ayrı istek (iki DI kapsamı, iki DbContext) aynı anda farklı teklifleri seçer.
        var outcomes = await Task.WhenAll(
            SelectAsync(s.CustomerId, s.ReportId, s.OfferIds[0]),
            SelectAsync(s.CustomerId, s.ReportId, s.OfferIds[1]));

        outcomes.Count(o => o == Outcome.Success).Should().Be(1);
        outcomes.Count(o => o == Outcome.Conflict409).Should().Be(1);

        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Offers.CountAsync(o => o.ReportId == s.ReportId && o.Status == OfferStatus.Accepted)).Should().Be(1);
        (await db.AccidentReports.SingleAsync(r => r.Id == s.ReportId)).Status.Should().Be(ReportStatus.OfferSelected);
    }

    [Fact]
    public async Task SelectThenUndo_PersistsSnapshotAndRestoresOffers()
    {
        var s = await Uc04TestData.CreateAsync(fixture);

        (await SelectAsync(s.CustomerId, s.ReportId, s.OfferIds[2])).Should().Be(Outcome.Success);

        await using (var scope = fixture.ScopeFor(s.CustomerId))
        {
            var undo = await scope.ServiceProvider.GetRequiredService<IOfferSelectionService>()
                .UndoAsync(new UndoSelectionRequest(s.ReportId), CancellationToken.None);
            undo.IsSuccess.Should().BeTrue();
            undo.Value.Status.Should().Be(ReportStatus.Open);
        }

        await using var check = fixture.Services.CreateAsyncScope();
        var db = check.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Offers.Where(o => o.ReportId == s.ReportId).Select(o => o.Status).ToListAsync())
            .Should().OnlyContain(status => status == OfferStatus.Pending);
        (await db.ReportStatusHistory.Where(h => h.ReportId == s.ReportId).OrderBy(h => h.Id).Select(h => h.CommandName).ToListAsync())
            .Should().Equal("SelectOfferCommand", "UndoSelectOffer");
    }

    // TC-04.4.2
    [Fact]
    public async Task Contact_NotMatchedMechanic_Gets403OrMasked()
    {
        // 2 usta teklif verdi, 1 usta eşleşti ama teklif vermedi.
        var s = await Uc04TestData.CreateAsync(fixture, mechanicCount: 2, matchedOnly: 1);
        (await SelectAsync(s.CustomerId, s.ReportId, s.OfferIds[0])).Should().Be(Outcome.Success);

        var winner = await ContactAsAsync(s.MechanicIds[0], s.ReportId);
        winner.Value.IsRevealed.Should().BeTrue();
        winner.Value.Phone.Should().Be("05321234567");

        var loser = await ContactAsAsync(s.MechanicIds[1], s.ReportId);
        loser.Value.IsRevealed.Should().BeFalse();
        loser.Value.Phone.Should().Be("05** *** ** 67");
        loser.Value.Email.Should().Be("-");

        var matchedWithoutOffer = await ContactAsAsync(s.MechanicIds[2], s.ReportId);
        matchedWithoutOffer.Value.IsRevealed.Should().BeFalse();
        matchedWithoutOffer.Value.Phone.Should().NotContain("1234");

        var stranger = await ContactAsAsync(Guid.NewGuid(), s.ReportId);
        stranger.Error!.Code.Should().Be(ErrorCodes.NotMatched);
    }

    private enum Outcome
    {
        Success,
        Conflict409
    }

    /// <summary>
    /// Bir seçim isteği. 409 üç yoldan gelebilir; hepsi API'de aynı yanıttır: xmin / unique index çakışması
    /// (ConflictException), rapor artık Open değil (DomainException) veya teklif artık Pending değil (Result).
    /// </summary>
    private async Task<Outcome> SelectAsync(Guid customerId, Guid reportId, Guid offerId)
    {
        await using var scope = fixture.ScopeFor(customerId);
        var service = scope.ServiceProvider.GetRequiredService<IOfferSelectionService>();
        try
        {
            var result = await service.SelectAsync(new SelectOfferRequest(reportId, offerId), CancellationToken.None);
            if (result.IsSuccess)
                return Outcome.Success;
            result.Error!.Type.Should().Be(ErrorType.Conflict);
            return Outcome.Conflict409;
        }
        catch (Exception ex) when (ex is ConflictException or DomainException)
        {
            return Outcome.Conflict409;
        }
    }

    private async Task<Result<Application.Contacts.Contracts.MatchContactResponse>> ContactAsAsync(Guid viewerId, Guid reportId)
    {
        await using var scope = fixture.ScopeFor(viewerId);
        return await scope.ServiceProvider.GetRequiredService<IContactService>().GetMatchContactAsync(reportId, CancellationToken.None);
    }
}
