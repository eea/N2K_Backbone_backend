namespace N2K_BackboneBackEnd.Services
{
    public interface IEULoginService
    {
        Task<string> GetLoginUrl(string redirectionUrl);
        Task<string> GetLoginUrl(string redirectionUrl, string code_challenge);
        Task<string> GetToken(string redirectionUri, string code, string code_verifier);
        Task<String> GetUsername(string token);
    }
}