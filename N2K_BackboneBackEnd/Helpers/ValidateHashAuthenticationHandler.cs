using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using N2K_BackboneBackEnd.Models;

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
            /*
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Header Not Found."));
            }
            var token = Request.Headers["Authorization"].ToString();
            token = token.Replace("Bearer ", "").Trim();
            */

            var token = "eyJraWQiOiJJTEp5VTQ5SEpNTXVodlNjVTJ0SUxfMGR4eENTa09OcEk4eElySzJnMWhrIiwiYWxnIjoiUlMyNTYifQ.eyJhdF9oYXNoIjoiUUE0Zmtsajd5X0hTRjRpTUxKeENiQSIsInN1YiI6Im4wMDA1YzA0IiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImFtciI6WyJwd2QiXSwiaXNzIjoiaHR0cHM6XC9cL2VjYXMuYWNjZXB0YW5jZS5lYy5ldXJvcGEuZXVcL2Nhc1wvb2F1dGgyIiwibm9uY2UiOiJhOWEwNWFlNC04MGUwLTQ4OWEtOTM5Zi04MzIzMzNlOWUwZTkiLCJhdWQiOiJnNXJKVVBxOTN1Z3pxN0pHOEJ0RTVsaUk5Y2F5dkhibUV6aEgxa0txTU94cVd4TkFsaVl6YmVlUGNKemUwem1WaWVpemZVRHdHTVpVNTFTekpKelhhbFNPLXpHcmNjN0dDT3k4WGJYYWxiS3gxcEMiLCJhY3IiOiJodHRwczpcL1wvZWNhcy5lYy5ldXJvcGEuZXVcL2xvYVwvYmFzaWMiLCJjX2hhc2giOiJURnJmMUJ5bHJMVWM0U01XZ1pVbkt3IiwiYXV0aF90aW1lIjoxNjYwNjQ0OTQ1LCJleHAiOjE2NjA2NTk0OTMsImlhdCI6MTY2MDY0NTA5MywiZW1haWwiOiJvZXNwYXJ6YUBiaWxib21hdGljYS5lcyJ9.X_9k2FJ7y9v_fc5SikM3sbaMkJljm9u57NTuBh9a7DuwhxrgmlymqRW9w0FhGURLWYxWJzTDqWGcT2CkHNHWbx5TxGCEyD2QynGtaJ8Zsi67UWCl6RqLy5YG09dL5a1EwHjhcKPClTC0RnkIkCMkVRQu8ktiT_wgCX3cXlcRqWj6jowDs78mX3dLOjVt0zVIlB3oyv_3yKdIHYNHphffOawtaX2U8jZF_VPoMvkESmfXwb1CZYn1xbRPtIOp_7cYdy7sHxZDtBeeM7tGX7_FVUTRPLGCEtR5vjPsqvLqaRwMu-J5Kw4LqcTl2iCax0lXV8dGkMDhViU8K5ywYx2U-WYGVdzaVrTZAPj2RMG3JJ34pbXFmM_UTcG7uXlI2vkOXsbjEm9DgzDshSuJvuWjBdpJDtQAHtaxQBlBRjwNRTCtpeqUBeHn_mtK_o8Mcr7ScFpK7guZBrSgi-QWdlxU8E3s7yyJxzlpsawKnx6HeJ_We0mYR8cCWZmzQSQ04Xu1n";
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

                //GlobalData.Username = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;
                GlobalData.Username = "oesparza@bilbomatica.es";

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
