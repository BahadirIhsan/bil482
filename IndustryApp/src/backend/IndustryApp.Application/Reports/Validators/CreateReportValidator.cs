using FluentValidation;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Reports.Contracts;
using Microsoft.Extensions.Options;

namespace IndustryApp.Application.Reports.Validators;

/// <summary>
/// Rapor isteğinin biçim kuralları (SDD 7.4). Eksik parça (fotoğraf, hasar, konum, araç türü) burada değil
/// AccidentReportBuilder.Build içinde REPORT_INCOMPLETE olarak reddedilir; kural tek yerde kalır (PAT-02.1).
/// </summary>
public sealed class CreateReportValidator : AbstractValidator<ICreateReportRequest>
{
    public const int MaxDamageItems = 30;

    public CreateReportValidator(IOptions<PhotosOptions> photos)
    {
        var maxPhotos = photos.Value.MaxCountPerReport;

        RuleFor(x => x.Description).MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.");
        RuleFor(x => x.City).MaximumLength(50).WithMessage("İl en fazla 50 karakter olabilir.");
        RuleFor(x => x.District).MaximumLength(80).WithMessage("İlçe en fazla 80 karakter olabilir.");
        RuleFor(x => x.District).NotEmpty().When(x => !x.IsDeviceLocation)
            .WithMessage("Konum izni verilmediğinde ilçe zorunludur.");
        RuleFor(x => x.AddressText).MaximumLength(300).WithMessage("Adres en fazla 300 karakter olabilir.");
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue)
            .WithMessage("Enlem -90 ile 90 arasında olmalıdır.");
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue)
            .WithMessage("Boylam -180 ile 180 arasında olmalıdır.");

        RuleFor(x => x.PhotoKeys!.Count).LessThanOrEqualTo(maxPhotos).When(x => x.PhotoKeys is not null)
            .OverridePropertyName(nameof(ICreateReportRequest.PhotoKeys))
            .WithMessage($"En fazla {maxPhotos} fotoğraf eklenebilir.");
        RuleFor(x => x.PhotoKeys).Must(keys => keys!.Distinct(StringComparer.Ordinal).Count() == keys!.Count)
            .When(x => x.PhotoKeys is not null)
            .WithMessage("Aynı fotoğraf iki kez eklenemez.");

        RuleFor(x => x.Damages!.Count).LessThanOrEqualTo(MaxDamageItems).When(x => x.Damages is not null)
            .OverridePropertyName(nameof(ICreateReportRequest.Damages))
            .WithMessage($"En fazla {MaxDamageItems} hasar bölgesi seçilebilir.");
        RuleForEach(x => x.Damages).ChildRules(item =>
        {
            item.RuleFor(d => d.CategoryId).GreaterThan(0).WithMessage("Hasar bölgesi seçilmelidir.");
            item.RuleFor(d => d.Severity).IsInEnum().WithMessage("Hasar şiddeti geçersiz.");
            item.RuleFor(d => d.Note).MaximumLength(300).WithMessage("Not en fazla 300 karakter olabilir.");
        });
    }
}
