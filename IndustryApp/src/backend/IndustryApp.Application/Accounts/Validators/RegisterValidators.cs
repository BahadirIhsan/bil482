using FluentValidation;
using IndustryApp.Application.Accounts.Contracts;

namespace IndustryApp.Application.Accounts.Validators;

/// <summary>
/// Kayıt formlarının ortak kuralları (T-8.4). Validator'lar interface üzerine yazılır; WebApi'deki
/// ValidationFilter isteğin uyguladığı interface'lerin validator'larını çalıştırır. Frontend'deki Zod şemaları
/// bu kurallarla aynıdır.
/// </summary>
public abstract class RegisterUserRules<T> : AbstractValidator<T>
    where T : IRegisterUserRequest
{
    public const string PhonePattern = @"^05\d{9}$";

    protected RegisterUserRules()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(150).WithMessage("Ad soyad en fazla 150 karakter olabilir.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.")
            .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir.");
        RuleFor(x => x.Phone).Matches(PhonePattern).WithMessage("Telefon 05XXXXXXXXX biçiminde olmalıdır.");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .Matches(@"\d").WithMessage("Şifre en az bir rakam içermelidir.")
            .Matches(@"\p{L}").WithMessage("Şifre en az bir harf içermelidir.");
        RuleFor(x => x.AcceptedLegalDocumentIds).NotEmpty().WithMessage("Zorunlu sözleşmeler onaylanmalıdır.");
    }
}

public sealed class RegisterCustomerValidator : RegisterUserRules<IRegisterCustomerRequest>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.City).NotEmpty().WithMessage("İl zorunludur.").MaximumLength(50);
        RuleFor(x => x.District).MaximumLength(80);
    }
}

public sealed class RegisterMechanicValidator : RegisterUserRules<IRegisterMechanicRequest>
{
    public RegisterMechanicValidator()
    {
        RuleFor(x => x.WorkshopName).NotEmpty().WithMessage("Atölye adı zorunludur.").MaximumLength(150);
        RuleFor(x => x.TaxNumber).Matches(@"^\d{10,11}$").WithMessage("Vergi numarası 10 veya 11 haneli olmalıdır.");
        RuleFor(x => x.City).NotEmpty().WithMessage("İl zorunludur.").MaximumLength(50);
        RuleFor(x => x.District).NotEmpty().WithMessage("İlçe zorunludur.").MaximumLength(80);
        RuleFor(x => x.Address).NotEmpty().WithMessage("Adres zorunludur.").MaximumLength(300);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage("Enlem -90 ile 90 arasında olmalıdır.");
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage("Boylam -180 ile 180 arasında olmalıdır.");
        RuleFor(x => x.SpecialtyCategoryIds).NotEmpty().WithMessage("En az bir uzmanlık seçilmelidir.");
    }
}

public sealed class LoginValidator : AbstractValidator<ILoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("E-posta zorunludur.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre zorunludur.");
    }
}
