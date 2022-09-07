namespace N2K_BackboneBackEnd.Models
{

    public class AttachedFilesConfig
    {
        public bool AzureBlob { get; set; }
        public string AzureConnectionString { get; set; } = "";
        public string FilesRootPath { get; set; } = "";
        public string JustificationFolder { get; set; } = "";
        public List<String> ExtensionWhiteList { get; set; } = new List<string>();
        public List<String> CompressionFormats { get; set; } = new List<string>();
        public string PublicFilesUrl { get; set; } = "";
    }

    public class ConfigSettings
    {
        public string client_id { get; set; } = "";
        public string client_secret { get; set; } = "";

        public int client_id_issued_at { get; set; }

        public string[] redirect_uris { get; set; } =new string[] { };
        public string authorisation_url { get; set; } = "";
        public string par_url { get; set; } = "";

        public string token_url { get; set; } = "";

        public int refresh_token_max_age { get; set; }

        public int id_token_max_age { get; set; }
        
        public bool InDevelopment { get; set; }

        public AttachedFilesConfig? AttachedFiles { get; set; }

        public string fme_security_token { get; set; } = "";
        public string fme_service_spatialload { get; set; } = "";
        public string fme_service_spatialchanges { get; set; } = "";
        public string fme_service_singlesite_spatialchanges { get; set; } = "";

    }
}
