using FluentAssertions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Offers;
using IndustryApp.Application.Offers.Contracts;
using IndustryApp.Application.Offers.Sorting;
using IndustryApp.Domain.Common;
using IndustryApp.UnitTests.Domain;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Offers;

/// <summary>UC-04 · FR-04.1 · Teklif listesi: yalnızca rapor sahibi görür, sıralamayı strateji belirler (PAT-04.4).</summary>
public sealed class OfferServiceTests
{
    private readonly ServiceTestContext _ctx = new();
    private readonly IOfferRepository _offers = Substitute.For<IOfferRepository>();
    private readonly OfferService _service;

    public OfferServiceTests()
    {
        _offers.ListForReportAsync(Arg.Any<Guid>(), Arg.Any<IOfferSortStrategy>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<OfferListItemResponse>());
        _service = new OfferService(
            _ctx.Reports, _offers, [new PriceAscendingSort(), new FastestCompletionSort(), new NewestFirstSort()], _ctx.CurrentUser);
    }

    [Fact]
    public async Task ListForReport_NotOwner_ReturnsForbidden()
    {
        var report = DomainTestData.OpenReport(customerId: Guid.NewGuid());
        _ctx.Reports.GetAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);

        var result = await _service.ListForReportAsync(new ReportOffersQuery(report.Id, "price"), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.NotReportOwner);
        await _offers.DidNotReceiveWithAnyArgs().ListForReportAsync(default, default!, default);
    }

    [Fact]
    public async Task ListForReport_MissingReport_ReturnsNotFound()
    {
        var result = await _service.ListForReportAsync(new ReportOffersQuery(Guid.NewGuid(), null), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.ReportNotFound);
    }

    [Theory]
    [InlineData("fastest", typeof(FastestCompletionSort))]
    [InlineData("NEWEST", typeof(NewestFirstSort))]
    [InlineData("price", typeof(PriceAscendingSort))]
    [InlineData(null, typeof(PriceAscendingSort))]
    [InlineData("rating", typeof(PriceAscendingSort))]
    public async Task ListForReport_Owner_UsesStrategyMatchingKeyOrPrice(string? sortKey, Type expected)
    {
        var report = DomainTestData.OpenReport(customerId: _ctx.UserId);
        _ctx.Reports.GetAsync(report.Id, Arg.Any<CancellationToken>()).Returns(report);

        var result = await _service.ListForReportAsync(new ReportOffersQuery(report.Id, sortKey), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _offers.Received(1).ListForReportAsync(
            report.Id, Arg.Is<IOfferSortStrategy>(s => s.GetType() == expected), Arg.Any<CancellationToken>());
    }
}
