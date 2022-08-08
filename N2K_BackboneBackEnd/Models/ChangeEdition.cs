namespace N2K_BackboneBackEnd.Models
{
    public class ChangeEdition
    {
        public string Sitecode { get; set; } = ""; 
        public int Version { get; set; }
        public string SiteName { get; set; } = "";
        public string SiteType { get; set; } = "";
        public int[] BiogeographicRegion { get; set; }
        public float Area { get; set; }
        public float Length { get; set; }
        public float CentreX { get; set; }
        public float CentreY { get; set; }

    }
}
