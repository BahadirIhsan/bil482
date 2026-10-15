using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Reports;

/// <summary>Enlem/boylam değer nesnesi; geçersiz koordinatla oluşturulamaz.</summary>
public readonly record struct GeoLocation
{
    public GeoLocation(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180 || double.IsNaN(latitude) || double.IsNaN(longitude))
            throw new DomainException(ErrorCodes.InvalidLocation, "Konum koordinatları geçersiz.");

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }

    public double Longitude { get; }
}
