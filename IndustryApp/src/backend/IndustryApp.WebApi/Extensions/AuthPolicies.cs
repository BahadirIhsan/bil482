namespace IndustryApp.WebApi.Extensions;

/// <summary>Yetki politikaları (T-11.3). Rol kontrolü attribute ile, kaynak sahipliği serviste yapılır.</summary>
public static class AuthPolicies
{
    public const string CustomerOnly = "CustomerOnly";
    public const string MechanicAny = "MechanicAny";
    public const string ApprovedMechanic = "ApprovedMechanic";
    public const string AdminOnly = "AdminOnly";
}
