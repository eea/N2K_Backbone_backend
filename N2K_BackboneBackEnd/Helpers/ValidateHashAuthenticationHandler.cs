using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace N2K_BackboneBackEnd.Helpers
{

    public class ValidateHashAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {

    }

    public class ValidateHashAuthenticationHandler: AuthenticationHandler<ValidateHashAuthenticationSchemeOptions>
    {
        public ValidateHashAuthenticationHandler(
            IOptionsMonitor<ValidateHashAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

     
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // validation comes in here
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Header Not Found."));
            }
            var token = Request.Headers["Authorization"].ToString();
            token = token.Replace("Bearer ", "").Trim();
            try
            {
                
                /*
                //Check if the token is active
                var appSettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                using (var client = new HttpClient())
                {
                    var acc = Base64Encode(String.Format("{0}:{1}",
                            appSettings.GetValue<string>("GeneralSettings:client_id"),
                            appSettings.GetValue<string>("GeneralSettings:client_secret")
                    ));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", acc);
                    client.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header


                    var values = new Dictionary<string, string>
                    {
                            {"token", token},
                            {"token_type_hint", "access_token" }
                    };
                    var content = new FormUrlEncodedContent(values);
                    var uri = "https://ecas.acceptance.ec.europa.eu/cas/oauth2/token/introspect";
                    var res = client.PostAsync(uri, content).Result;
                    var json = res.Content.ReadAsStringAsync().Result;
                    
                    JObject jResponse = JObject.Parse(json);
                    var active = false;
                    if (jResponse.ContainsKey("active")) {
                        active = bool.Parse(jResponse.GetValue("active").ToString());
                    }
                }
                */


                //Check if the token is valid
                var tokenHandler = new JwtSecurityTokenHandler();                                
                var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

                var claimsIdentity = new ClaimsIdentity(jwtSecurityToken.Claims,
                                nameof(ValidateHashAuthenticationHandler));

                // generate AuthenticationTicket from the Identity
                // and current authentication scheme
                var ticket = new AuthenticationTicket(
                    new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);

                // pass on the ticket to the middleware
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Exception Occured while Deserializing: " + ex);
                return Task.FromResult(AuthenticateResult.Fail("TokenParseException"));
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
