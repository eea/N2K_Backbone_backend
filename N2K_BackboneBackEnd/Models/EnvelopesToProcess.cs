namespace N2K_BackboneBackEnd.Models
{
    public class EnvelopesToProcess
    {
        public int VersionId { get; set; }
        public string CountryCode { get; set; } = "";

        public DateTime SubmissionDate { get; set; }

        public long JobId { get;set; }
    }
}
