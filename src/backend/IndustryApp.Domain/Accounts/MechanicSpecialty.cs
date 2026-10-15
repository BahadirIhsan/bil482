namespace IndustryApp.Domain.Accounts;

/// <summary>Ustanın uzmanlığı; yalnızca 1. seviye hasar kategorisine bağlanır (T-6.4).</summary>
public sealed class MechanicSpecialty
{
    private MechanicSpecialty()
    {
    }

    internal MechanicSpecialty(Guid mechanicId, int damageCategoryId)
    {
        MechanicId = mechanicId;
        DamageCategoryId = damageCategoryId;
    }

    public Guid MechanicId { get; private set; }

    public int DamageCategoryId { get; private set; }
}
