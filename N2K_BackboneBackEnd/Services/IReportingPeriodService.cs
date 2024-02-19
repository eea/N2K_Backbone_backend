using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Services
{
    public interface IReportingPeriodService
    {
        Task<List<RepPeriodView>> Get();
        Task<List<RepPeriodView>> Edit(long id, DateTime newEndDate);
        Task<List<RepPeriodView>> Create(DateTime init, DateTime end);
        Task<List<RepPeriodView>> Close();
    }
}
