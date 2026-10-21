using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Catalog;

/// <summary>Araç türü (Otomobil, SUV ...). Referans veridir.</summary>
public sealed class VehicleType : BaseEntity<short>
{
    private VehicleType()
    {
    }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public short SortOrder { get; private set; }

    public bool IsActive { get; private set; }
}
