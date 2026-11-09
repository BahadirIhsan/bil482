using IndustryApp.Application.Contacts;
using IndustryApp.Application.Contacts.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>UC-04 eşleşme iletişim bilgisi (endpoint 32). Maskeleme kararı ContactInfoProxy'dedir (PAT-04.3).</summary>
[Route("api/reports/{reportId:guid}/contact")]
[Authorize]
public sealed class ContactsController(IContactService contacts) : ApiControllerBase
{
    /// <summary>32 · Karşı tarafın iletişim bilgisi; eşleşmemiş kullanıcı 403 veya maskeli veri alır.</summary>
    [HttpGet]
    [ProducesResponseType<MatchContactResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid reportId, CancellationToken ct) =>
        ToActionResult(await contacts.GetMatchContactAsync(reportId, ct));
}
