namespace N2K_BackboneBackEnd.Models
{
    public class GetTokenParams
    {
        public string RedirectionUrl { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Code_Verifier { get; set; } = string.Empty;
    }

    public class GetUsernameParams
    {
        public string Token { get; set; } = string.Empty;
    }
}