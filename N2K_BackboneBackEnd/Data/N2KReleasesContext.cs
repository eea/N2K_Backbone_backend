using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KReleasesContext : BaseContext
    {
        public N2KReleasesContext(DbContextOptions<N2KReleasesContext> options) : base(options)
        {

        }

    }
}