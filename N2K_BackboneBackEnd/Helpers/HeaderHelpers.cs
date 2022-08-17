using System.IdentityModel.Tokens.Jwt;

namespace N2K_BackboneBackEnd.Helpers
{
    public class HeaderHelpers
    {
        public static string GetUsername(IHeaderDictionary headers)
        {
            if (!headers.ContainsKey("Authorization"))
                return "";
            var email = "";
            try
            {
                var token = headers["Authorization"].ToString();
                token = token.Replace("Bearer ", "").Trim();
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                email = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;
            }
            catch (Exception e) { }
            return email;
        }
    }
}
