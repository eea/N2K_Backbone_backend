using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Services
{
    public interface IReportingPeriodService
    {
        Task<List<RepPeriodView>> Get();
        Task<List<RepPeriodView>> Edit(RepPeriod rp);
        Task<List<RepPeriodView>> Create(RepPeriod rp);
        Task<List<RepPeriodView>> Close();
    }
}
