namespace N2K_BackboneBackEnd.Models
{
    public class AttachedFilesConfig
    {
        public bool AzureBlob { get; set; }
        public string AzureConnectionString { get; set; } = string.Empty;
        public string FilesRootPath { get; set; } = string.Empty;
        public string JustificationFolder { get; set; } = string.Empty;
        public List<String> ExtensionWhiteList { get; set; } = new List<string>();
        public List<String> CompressionFormats { get; set; } = new List<string>();
        public string PublicFilesUrl { get; set; } = string.Empty;
    }

    public class fme_service_config
    {
        public string server_url { get; set; } = string.Empty;
        public string repository { get; set; } = string.Empty;
        public string workspace { get; set; } = string.Empty;
    }

    public class ConfigSettings
    {
        public string client_id { get; set; } = string.Empty;
        public string client_secret { get; set; } = string.Empty;
        public int client_id_issued_at { get; set; }
        public string[] redirect_uris { get; set; } = new string[] { };
        public string authorisation_url { get; set; } = string.Empty;
        public string par_url { get; set; } = string.Empty;
        public string token_url { get; set; } = string.Empty;
        public int refresh_token_max_age { get; set; }
        public int id_token_max_age { get; set; }
        public bool InDevelopment { get; set; }
        public string Environment { get; set; } = string.Empty;
        public string ReleaseDestDatasetFolder  { get; set; } = string.Empty;
        public AttachedFilesConfig? AttachedFiles { get; set; }
        public string fme_security_token { get; set; } = string.Empty;
        public fme_service_config fme_service_spatialload { get; set; }
        public fme_service_config fme_service_release { get; set; }
        public string fme_service_spatialchanges { get; set; } = string.Empty;
        public string fme_service_singlesite_spatialchanges { get; set; } = string.Empty;
        public string current_ul_name { get; set; } = string.Empty;
        public string current_ul_createdby { get; set; } = string.Empty;
        public string fme_service_extractions { get; set; } = string.Empty;
        public string fme_service_union_lists { get; set; } = string.Empty;
    }
}