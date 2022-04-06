using System.ComponentModel.DataAnnotations;
using N2K_BackboneBackEnd.Enumerations;

namespace N2K_BackboneBackEnd.Models
{
    public class SiteChange
    {
        [Key]
        public int ChangeId { get; set; }

        public string? SiteCode { get; set;  }
        public string? Country { get; set; }

        public Status? Status { get; set; }

        public string? Tags { get; set; }
        
        public Level? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }

    }
}
