using FluentAssertions;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Reports;

namespace IndustryApp.UnitTests.Domain;

/// <summary>PAT-02.1 Builder: eksik rapor kurulamaz.</summary>
public sealed class AccidentReportBuilderTests
{
    // TC-02.3.1
    [Fact]
    public void Builder_MissingPhoto_Throws()
    {
        var builder = new AccidentReportBuilder()
            .WithCustomer(Guid.NewGuid())
            .WithVehicleType(1)
            .AtLocation("Ankara", "Çankaya", 39.92, 32.85)
            .AddDamage(DomainTestData.Category(), DamageSeverity.Medium);

        var act = () => builder.Build(DomainTestData.Now, DomainTestData.ReportTtl);

        act.Should().Throw<DomainException>()
            .Where(e => e.Code == ErrorCodes.ReportIncomplete && e.Message.Contains("fotoğraf"));
    }
}
