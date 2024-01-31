namespace N2K_BackboneBackEnd.Services
{
    public interface IConfigService
    {
        Task<String> GetFrontEndConfiguration();
    }
}