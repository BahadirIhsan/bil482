namespace IndustryApp.Domain.Accounts;

/// <summary>Sistemdeki roller; adlar roles tablosundaki referans veriyle birebir aynıdır.</summary>
public static class RoleNames
{
    public const string Customer = "Customer";
    public const string Mechanic = "Mechanic";
    public const string Admin = "Admin";

    public static IReadOnlyList<string> All { get; } = [Customer, Mechanic, Admin];
}
