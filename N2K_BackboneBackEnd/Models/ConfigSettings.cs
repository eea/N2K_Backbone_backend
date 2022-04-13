namespace N2K_BackboneBackEnd.Models
{
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

    }
}
