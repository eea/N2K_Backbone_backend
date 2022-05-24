
using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Data
{
    public class N2KBackboneReadOnlyContext : BaseContext
    {
        public N2KBackboneReadOnlyContext(DbContextOptions<N2KBackboneReadOnlyContext> options) : base(options) { }
    }
}