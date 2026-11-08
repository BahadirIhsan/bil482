using FluentAssertions;
using IndustryApp.Domain.Contacts;

namespace IndustryApp.UnitTests.Domain;

/// <summary>PAT-04.3 Proxy: eşleşmemiş kullanıcı gerçek iletişim bilgisini göremez.</summary>
public sealed class ContactInfoProxyTests
{
    // TC-04.4.1
    [Fact]
    public void ContactProxy_NotMatched_ReturnsMaskedPhone()
    {
        var customerId = Guid.NewGuid();
        var report = DomainTestData.OpenReport(customerId);
        var real = new ContactInfo("Ayşe Yılmaz", "05321234567", "ayse@test.local");
        var outsider = Guid.NewGuid();

        var proxy = new ContactInfoProxy(real, report, outsider);

        proxy.IsRevealed.Should().BeFalse();
        proxy.Phone.Should().Be("05** *** ** 67");
        proxy.Phone.Should().NotContain("1234");
        proxy.Email.Should().Be("-");
    }
}
