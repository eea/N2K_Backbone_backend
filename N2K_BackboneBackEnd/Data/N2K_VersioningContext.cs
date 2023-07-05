using Microsoft.EntityFrameworkCore;


namespace N2K_BackboneBackEnd.Data
{
    public class N2K_VersioningContext : BaseContext
    {
        //public DbSet<OwnerType> OwnerType { get; set; }

        public N2K_VersioningContext(DbContextOptions<N2K_VersioningContext> options) : base(options) {
            this.Database.SetCommandTimeout(600);
        }

    }
}
