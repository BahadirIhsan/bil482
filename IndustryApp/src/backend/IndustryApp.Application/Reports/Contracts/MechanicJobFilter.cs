namespace IndustryApp.Application.Reports.Contracts;

/// <summary>Ustanın iş listesi sekmeleri (UC-03).</summary>
public enum MechanicJobFilter
{
    /// <summary>Eşleşmiş, teklif verilmemiş, rapor açık.</summary>
    New,

    /// <summary>Teklif verilmiş ve karar bekliyor.</summary>
    Offered,

    /// <summary>Teklif seçildi.</summary>
    Won,

    /// <summary>Başka teklif seçildi.</summary>
    Lost
}
