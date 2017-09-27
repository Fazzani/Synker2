namespace hfa.WebApi.Common.Auth
{
    public interface IAuthentificationService
    {
        string Authenticate(string username, string password);
        string Salt { get; }
    }
}