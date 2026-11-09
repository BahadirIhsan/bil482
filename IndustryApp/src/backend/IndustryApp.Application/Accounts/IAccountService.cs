using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;

namespace IndustryApp.Application.Accounts;

/// <summary>
/// Oturumdaki kullanıcının hesabı (endpoint 6). Profil güncelleme (8, 9), müsaitlik (10) ve rol paneli (7,
/// Abstract Factory PAT-01.2) sonraki sürümlerde eklenir.
/// </summary>
public interface IAccountService
{
    Task<Result<MeResponse>> GetMeAsync(CancellationToken ct);
}
