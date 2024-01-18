using Microsoft.EntityFrameworkCore;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    [Keyless]
    public class LineageCount : IEntityModelBackboneDB
    {
        public int Proposed { get; set; }
        public int Consolidated { get; set; }
    }
}