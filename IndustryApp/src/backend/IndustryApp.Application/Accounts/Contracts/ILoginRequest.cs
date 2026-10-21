namespace IndustryApp.Application.Accounts.Contracts;

public interface ILoginRequest
{
    string Email { get; }

    string Password { get; }
}
