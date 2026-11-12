using FluentAssertions;
using IndustryApp.Domain.Catalog;
using IndustryApp.Domain.Common;
using IndustryApp.UnitTests.Application;

namespace IndustryApp.UnitTests.Domain;

/// <summary>PAT-02.2 Composite: dal ve yaprak aynı arayüzle dolaşılır.</summary>
public sealed class DamageCategoryTests
{
    // TC-02.2.1
    [Fact]
    public void DamageCategory_Contains_FindsDescendant()
    {
        var body = DamageCategory.Create("BODY", "Kaporta", 1).WithId(1);
        var fender = DamageCategory.Create("BODY_FENDER", "Çamurluk", 1, body).WithId(2);
        var leftFront = DamageCategory.Create("BODY_FENDER_LF", "Sol ön çamurluk", 1, fender).WithId(3);
        var other = DamageCategory.Create("GLASS", "Cam", 2).WithId(4);

        IDamageComponent root = body;
        root.Contains(leftFront.Id).Should().BeTrue();
        root.Contains(other.Id).Should().BeFalse();
        body.Find(3).Should().BeSameAs(leftFront);
        leftFront.IsLeaf.Should().BeTrue();
        root.IsLeaf.Should().BeFalse();
    }

    [Fact]
    public void Create_FourthLevel_ThrowsMaxDepth()
    {
        var l1 = DamageCategory.Create("A", "A", 1);
        var l2 = DamageCategory.Create("A_B", "B", 1, l1);
        var l3 = DamageCategory.Create("A_B_C", "C", 1, l2);

        var act = () => DamageCategory.Create("A_B_C_D", "D", 1, l3);

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.CategoryMaxDepth);
    }
}
