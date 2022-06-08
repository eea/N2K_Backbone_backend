using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.ViewModel;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Enumerations;
using N2K_BackboneBackEnd.Models.versioning_db;


namespace N2K_BackboneBackEnd.Services
{
    public class SiteDetailsService: ISiteDetailsService
    {

        private readonly N2KBackboneContext _dataContext;

        public SiteDetailsService(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
        }


        #region SiteComments

        public async Task<List<StatusChanges>> ListSiteComments(string pSiteCode, int pCountryVersion)
        {
            List<StatusChanges> result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == pSiteCode && ch.Version == pCountryVersion).ToListAsync();
            return result;
        }


        public async Task<List<StatusChanges>> AddComment(StatusChanges comment)
        {

            comment.Date = DateTime.Now;
            await _dataContext.Set<StatusChanges>().AddAsync(comment);
            await _dataContext.SaveChangesAsync();

            List<StatusChanges> result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == comment.SiteCode && ch.Version == comment.Version).ToListAsync();
            return result;

        }

        public async Task<int> DeleteComment(int CommentId)
        {
            int result = 0;
            StatusChanges comment = await _dataContext.Set<StatusChanges>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == CommentId);
            if (comment != null)
            {
                _dataContext.Set<StatusChanges>().Remove(comment);
                await _dataContext.SaveChangesAsync();
                result = 1;
            }
            return result;
        }

        public async Task<List<StatusChanges>> UpdateComment(StatusChanges comment)
        {
            _dataContext.Set<StatusChanges>().Update(comment);
            await _dataContext.SaveChangesAsync();

            List<StatusChanges> result = await _dataContext.Set<StatusChanges>().AsNoTracking().Where(ch => ch.SiteCode == comment.SiteCode && ch.Version == comment.Version).ToListAsync();
            return result;

        }



        #endregion

        #region SiteFiles

        public async Task<List<JustificationFiles>> ListSiteFiles(string pSiteCode, int pCountryVersion)
        {
            List<JustificationFiles> result = await _dataContext.Set<JustificationFiles>().AsNoTracking().Where(f => f.SiteCode == pSiteCode && f.Version == pCountryVersion).ToListAsync();
            return result;
        }

        #endregion
    }
}
