namespace IndustryApp.Application.Common;

/// <summary>Hata türü; ApiControllerBase bunu HTTP koduna çevirir (T-11.4).</summary>
public enum ErrorType
{
    /// <summary>400</summary>
    Validation,

    /// <summary>401</summary>
    Unauthorized,

    /// <summary>403</summary>
    Forbidden,

    /// <summary>404</summary>
    NotFound,

    /// <summary>409</summary>
    Conflict,

    /// <summary>422</summary>
    BusinessRule
}
