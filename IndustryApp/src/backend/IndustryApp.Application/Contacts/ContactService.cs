using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Contacts.Contracts;
using IndustryApp.Domain.Contacts;

namespace IndustryApp.Application.Contacts;

/// <summary>
/// Proxy'nin istemcisi (PAT-04.3). Gerçek iletişim bilgisi IIdentityService'ten alınır ve dışarıya HER ZAMAN
/// ContactInfoProxy ile sarılarak verilir; gerçek numaraya bu sınıfta proxy dışından erişilmez. Göstermeye
/// karar veren tek yer proxy'dir: report.IsMatchedWith(viewerId).
/// </summary>
public sealed class ContactService(
    IAccidentReportRepository reports,
    IMechanicProfileRepository mechanics,
    IIdentityService identity,
    ICurrentUser currentUser) : IContactService
{
    public async Task<Result<MatchContactResponse>> GetMatchContactAsync(Guid reportId, CancellationToken ct)
    {
        var report = await reports.GetWithOffersAsync(reportId, ct);
        if (report is null)
            return Errors.Reports.NotFound;

        var viewerId = currentUser.UserId;
        var isOwner = report.IsOwnedBy(viewerId);
        var isMechanicParty = report.Matches.Any(m => m.MechanicId == viewerId) || report.Offers.Any(o => o.MechanicId == viewerId);
        if (!isOwner && !isMechanicParty)
            return Errors.Offers.NotMatched; // raporla hiç ilgisi olmayan kullanıcı → 403

        Guid counterpartId;
        ContactCounterpartRole role;
        if (isOwner)
        {
            var winnerId = report.Offers.FirstOrDefault(o => o.Id == report.SelectedOfferId)?.MechanicId;
            if (winnerId is null)
                return Errors.Selection.NoActiveSelection;
            counterpartId = winnerId.Value;
            role = ContactCounterpartRole.Mechanic;
        }
        else
        {
            counterpartId = report.CustomerId;
            role = ContactCounterpartRole.Customer;
        }

        var contact = await identity.GetContactAsync(counterpartId, ct);
        if (contact is null)
            return Errors.Contacts.NotFound;

        IContactInfo info = new ContactInfoProxy(new ContactInfo(contact.FullName, contact.Phone, contact.Email), report, viewerId);

        string? workshopName = null;
        string? address = null;
        if (role == ContactCounterpartRole.Mechanic)
        {
            var profile = await mechanics.GetAsync(counterpartId, ct);
            workshopName = profile?.WorkshopName;
            address = info.IsRevealed ? profile?.Address : null;
        }

        return new MatchContactResponse(report.Id, role, info.FullName, info.Phone, info.Email, info.IsRevealed, workshopName, address);
    }
}
