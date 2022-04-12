namespace N2K_BackboneBackEnd.Services
{
    public interface IEULoginService
    {

        Task<string> GetLoginUrl(string redirectionUrl);

        Task<String> GetUsername(string token);

        Task<int> Logout(string token);

        
    }
}
