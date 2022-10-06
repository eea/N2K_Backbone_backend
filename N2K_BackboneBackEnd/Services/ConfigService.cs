using N2K_BackboneBackEnd.Helpers;

namespace N2K_BackboneBackEnd.Services
{
    public class ConfigService : IConfigService
    {
        public async Task<string> GetFrontEndConfiguration()
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "front_end_config.json");
                string text =await File.ReadAllTextAsync(path);
                return text;
            }
            catch (Exception ex)
            {
                return  await Task.FromResult(ex.Message);
            }

        }
    }
}
