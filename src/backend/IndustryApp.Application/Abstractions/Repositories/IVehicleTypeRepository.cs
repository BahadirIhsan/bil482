using IndustryApp.Domain.Catalog;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface IVehicleTypeRepository
{
    Task<IReadOnlyList<VehicleType>> ListActiveAsync(CancellationToken ct);

    /// <summary>Aktif bir araç türü var mı.</summary>
    Task<bool> ExistsAsync(short id, CancellationToken ct);
}
