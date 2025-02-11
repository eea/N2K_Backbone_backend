using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace N2K_BackboneBackEnd.Services
{
    public class EULoginService : IEULoginService
    {
        private readonly IOptions<ConfigSettings> _appSettings;

        private static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static Random random = new();

        private static string generateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string generateCodeChallenge(string code_verifier)
        {
            return Base64Encode(ComputeSha256Hash(code_verifier));
        }

        public EULoginService(IOptions<ConfigSettings> app)
        {
            _appSettings = app;
        }

        public async Task<string> GetLoginUrl(string redirectionUrl)
        {
            return await GetLoginUrl(redirectionUrl, generateRandomString(128));
        }

        public async Task<string> GetLoginUrl(string redirectionUrl, string code_challenge)
        {
            /*** GENERATE THE JWT token object */
            string key = _appSettings.Value.client_secret;
            // Create Security key  using private key above:
            // not that latest version of JWT using Microsoft namespace instead of System
            var securityKey = new Microsoft
                .IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Also note that securityKey length should be >256b
            // so you have to make sure that your private key has a proper length
            //
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials
                                  (securityKey, SecurityAlgorithms.HmacSha256);

            //  Finally create a Token
            var header = new JwtHeader(credentials);

            //build the JSON data payload for the JWT token
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            var payload = new JwtPayload
                {
                 {"code_challenge_method" , "S256" },
                 {"code_challenge",code_challenge },
                 {"exp" , secondsSinceEpoch},
                 {"aud", _appSettings.Value.par_url},
                 { "iss", _appSettings.Value.client_id },
                 {"nonce", Guid.NewGuid().ToString()},
                 {"jti" , Guid.NewGuid().ToString()}
                };

            //combine JWTHeader and JWTPayload
            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            // Token to String so you can use it in your client
            var tokenString = handler.WriteToken(secToken);

            using (HttpClient client = new())
            {
                //build the POST request so that we can obtain the request_uri
                var acc = Base64Encode(String.Format("{0}:{1}", _appSettings.Value.client_id, _appSettings.Value.client_secret));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", acc);
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header

                var values = new Dictionary<string, string>
                    {
                            {"response_type", "code" },
                            {"scope", "openid email" },
                            {"client_id", _appSettings.Value.client_id },
                            {"redirect_uri",redirectionUrl },
                            {"request",tokenString }
                    };
                var content = new FormUrlEncodedContent(values);
                var uri = _appSettings.Value.par_url;

                try
                {
                    var res = await client.PostAsync(uri, content);
                    var json = await res.Content.ReadAsStringAsync();
                    var response_dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    var requestUri = "";
                    if (response_dict.ContainsKey("request_uri"))
                    {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                        requestUri = response_dict["request_uri"];
                    }
                    else
                    {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                        requestUri = response_dict["error_description"];
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                    }
                    res.Dispose();
                    return String.Format(@"{0}?client_id={1}&response_type=code&request_uri={2}",
                        _appSettings.Value.authorisation_url,
                        _appSettings.Value.client_id,
                        requestUri);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        public async Task<string> GetToken(string redirectionUri, string code, string code_verifier)
        {
            using (HttpClient client = new())
            {
                //build the POST request so that we can obtain the request_uri
                var acc = Base64Encode(String.Format("{0}:{1}", _appSettings.Value.client_id, _appSettings.Value.client_secret));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", acc);
                //client.DefaultRequestHeaders.Accept
                //    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header

                var values = new Dictionary<string, string>
                    {
                            {"grant_type", "authorization_code" },
                            {"code", code },
                            {"code_verifier",  code_verifier },
                            {"refresh_token_max_age",_appSettings.Value.refresh_token_max_age.ToString() },
                            {"id_token_max_age", _appSettings.Value.id_token_max_age.ToString()},
                            {"redirect_uri",redirectionUri},
                            {"version_ui_required", "false" }
                    };
                var content = new FormUrlEncodedContent(values);
                var uri = _appSettings.Value.token_url;

                try
                {
                    var res = await client.PostAsync(uri, content);
                    var json = await res.Content.ReadAsStringAsync();

                    var response_dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    var requestUri = "";

                    if (response_dict.ContainsKey("id_token"))
                    {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                        requestUri = response_dict["id_token"];
                    }
                    else
                    {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                        requestUri = response_dict["error_description"];
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                    }
                    res.Dispose();
                    return String.Format(@"{0}", requestUri);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        public async Task<string> GetUsername(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var email = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;

            return await Task.FromResult(email);
        }
    }
}