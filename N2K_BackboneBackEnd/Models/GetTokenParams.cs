namespace N2K_BackboneBackEnd.Models
{
    public class GetTokenParams
    {
        public string RedirectionUrl { get; set; } = "";
        public string Code { get; set; } = "";
        public string Code_Verifier { get; set; } = "";
    }

    public class GetUsernameParams
    {
        public string Token { get; set; } = "";

    }
}
