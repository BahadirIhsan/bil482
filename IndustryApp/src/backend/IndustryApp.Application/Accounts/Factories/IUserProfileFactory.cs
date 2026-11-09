using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;

namespace IndustryApp.Application.Accounts.Factories;

// [UC-01][PAT-01.1] Factory Method
// Niyet   : Kayıt akışını tek tutup role özgü profilin (müşteri, usta) üretimini alt sınıflara bırakmak.
// Neden   : Müşteri ve usta kaydı aynı adımları izler (sözleşme, kimlik, profil, onay kaydı) ama farklı profil,
//           farklı zorunlu sözleşme ve farklı kurallar ister. AuthService içinde if (role == …) blokları her yeni
//           rolde kayıt servisini değiştirirdi.
// Kazanım : AuthService.RegisterAsync role bakmaz; Role anahtarına uyan fabrikayı DI'dan seçer. Zorunlu sözleşme
//           kodları da fabrikadadır, kayıt akışı rol bilgisini tek yerden alır.
// Metrik  : Yeni rol = 1 yeni fabrika, 0 değişen sınıf (Register_Customer_CreatesCustomerProfile,
//           Register_Mechanic_CreatesPendingProfile, Register_UnknownRole_ReturnsUnknownRole).
/// <summary>Rol başına profil fabrikası. Her uygulama DI'a IUserProfileFactory olarak kaydedilir.</summary>
public interface IUserProfileFactory
{
    string Role { get; }

    /// <summary>Kayıtta onaylanması zorunlu sözleşmelerin kodları (legal_documents.code).</summary>
    IReadOnlyCollection<string> RequiredLegalDocumentCodes { get; }

    /// <summary>Kimliği oluşturulmuş kullanıcının profilini ekler; kaydetmez (UnitOfWork çağıran taraftadır).</summary>
    Task<Result> CreateProfileAsync(Guid userId, IRegisterUserRequest request, CancellationToken ct);
}
