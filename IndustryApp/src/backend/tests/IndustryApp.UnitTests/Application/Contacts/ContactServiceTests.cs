using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Contacts;
using IndustryApp.Application.Contacts.Contracts;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Reports;
using IndustryApp.UnitTests.Domain;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Contacts;

/// <summary>PAT-04.3 Proxy (servis seviyesi): gerçek numara yalnızca müşteri ve seçilen ustaya açılır (FR-04.4).</summary>
public sealed class ContactServiceTests
{
    private const string CustomerPhone = "05321234567";
    private const string MechanicPhone = "05449876543";

    private readonly ServiceTestContext _ctx = new();
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IMechanicProfileRepository _mechanics = Substitute.For<IMechanicProfileRepository>();
    private readonly ContactService _service;

    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _winnerId = Guid.NewGuid();
    private readonly Guid _loserId = Guid.NewGuid();
    private readonly AccidentReport _report;

    public ContactServiceTests()
    {
        _service = new ContactService(_ctx.Reports, _mechanics, _identity, _ctx.CurrentUser);

        _report = DomainTestData.OpenReport(_customerId);
        _report.RecordMatches("CITY", [(_winnerId, null), (_loserId, null)], DomainTestData.Now);
        var winning = DomainTestData.OfferFor(_report, _winnerId);
        DomainTestData.OfferFor(_report, _loserId, labor: 4500m);
        _report.SelectOffer(winning, DomainTestData.Now.AddMinutes(10));
        _ctx.Returns(_report);

        // Sahte kişi önce kurulur; Returns() içinde başka bir substitute yapılandırılamaz.
        var customerContact = Contact(_customerId, "Ayşe Yılmaz", CustomerPhone);
        var winnerContact = Contact(_winnerId, "Mehmet Usta", MechanicPhone);
        _identity.GetContactAsync(_customerId, Arg.Any<CancellationToken>()).Returns(customerContact);
        _identity.GetContactAsync(_winnerId, Arg.Any<CancellationToken>()).Returns(winnerContact);
    }

    [Fact]
    public async Task Contact_SelectedMechanic_SeesRealCustomerPhone()
    {
        _ctx.UserId = _winnerId;

        var result = await _service.GetMatchContactAsync(_report.Id, CancellationToken.None);

        result.Value.CounterpartRole.Should().Be(ContactCounterpartRole.Customer);
        result.Value.IsRevealed.Should().BeTrue();
        result.Value.Phone.Should().Be(CustomerPhone);
    }

    [Fact]
    public async Task Contact_Customer_SeesWinningMechanic()
    {
        _ctx.UserId = _customerId;

        var result = await _service.GetMatchContactAsync(_report.Id, CancellationToken.None);

        result.Value.CounterpartRole.Should().Be(ContactCounterpartRole.Mechanic);
        result.Value.IsRevealed.Should().BeTrue();
        result.Value.Phone.Should().Be(MechanicPhone);
    }

    // TC-04.4.2 (servis seviyesi)
    [Fact]
    public async Task Contact_NotSelectedMechanic_GetsMaskedPhone()
    {
        _ctx.UserId = _loserId;

        var result = await _service.GetMatchContactAsync(_report.Id, CancellationToken.None);

        result.Value.IsRevealed.Should().BeFalse();
        result.Value.Phone.Should().Be("05** *** ** 67").And.NotContain("1234");
        result.Value.Email.Should().Be("-");
    }

    [Fact]
    public async Task Contact_UnrelatedUser_ReturnsForbidden()
    {
        _ctx.UserId = Guid.NewGuid();

        var result = await _service.GetMatchContactAsync(_report.Id, CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.NotMatched);
        await _identity.DidNotReceive().GetContactAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static IUserContact Contact(Guid id, string name, string phone)
    {
        var contact = Substitute.For<IUserContact>();
        contact.UserId.Returns(id);
        contact.FullName.Returns(name);
        contact.Phone.Returns(phone);
        contact.Email.Returns($"{id:N}@test.local");
        return contact;
    }
}
