

using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KBackboneContext : BaseContext
    {
        public N2KBackboneContext(DbContextOptions<N2KBackboneContext> options) :  base(options) {
            this.Database.SetCommandTimeout(600);
        }

    }
}
