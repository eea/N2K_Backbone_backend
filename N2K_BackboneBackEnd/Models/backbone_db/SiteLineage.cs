using Microsoft.EntityFrameworkCore;
using N2K_BackboneBackEnd.Models.ViewModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace N2K_BackboneBackEnd.Models.backbone_db
{
    public class SiteLineage : IEntityModel
    {
        public string? SiteCode { get; set; }
        public string? Version { get; set; }
        [NotMapped]
        public SiteLineageView? Predecessors { get; set; } = new SiteLineageView();
        [NotMapped]
        public SiteLineageView? Successors { get; set; } = new SiteLineageView();
        public SiteLineage()
        {
            this.Predecessors = new SiteLineageView();
            this.Successors = new SiteLineageView();
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SiteLineage>().HasNoKey();
        }
    }

    

}
