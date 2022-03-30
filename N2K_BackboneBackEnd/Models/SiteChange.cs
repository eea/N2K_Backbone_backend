namespace N2K_BackboneBackEnd.Models
{
    public class SiteChange
    {
        public string? SiteCode { get; set;  }
        public string? Country { get; set; }
        public string? Status { get; set; } 

        public string? Tags { get; set; }
        public int ChangeId { get; set; }
        public string? Level { get; set; }
        public string? ChangeCategory { get; set; }
        public string? ChangeType { get; set; }

    }
}
