namespace N2K_BackboneBackEnd.Models
{
    public class EnvelopesToProcess
    {
        public int VersionId { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public long JobId { get; set; }
    }
}