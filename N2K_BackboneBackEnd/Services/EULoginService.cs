using AutoMapper;
using Microsoft.Extensions.Options;
using N2K_BackboneBackEnd.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace N2K_BackboneBackEnd.Services
{
    public class EULoginService : IEULoginService
    {
        /*
        private readonly IOptions<ConfigSettings> _appSettings;
        private readonly IEULoginService _euLoginService;
        private readonly IMapper _mapper;
        */
        /*
        public EULoginService(IOptions<ConfigSettings> app, IEULoginService euLoginService, IMapper mapper)
        {
            _appSettings = app;
            _euLoginService = euLoginService;
            _mapper = mapper;
        }
        */

        private readonly IOptions<ConfigSettings> _appSettings;

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
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


        private static Random random = new Random();

        public static string generateRandomString(int length)
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
            var code_challenge = generateCodeChallenge(generateRandomString(128));
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            var payload = new JwtPayload
                {
                 {"code_challenge_method" , "S256" },
                 {"code_challenge",code_challenge },
                 {"exp" , secondsSinceEpoch},
                 //{"iat" , iat},  
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

            using (var client = new HttpClient()) { 
                //build the POST request so that we can obtain the request_uri
                var acc = Base64Encode(String.Format("{0}:{1}", _appSettings.Value.client_id, _appSettings.Value.client_secret));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", acc);
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));//ACCEPT header

                var values = new Dictionary<string, string>
                    {
                            { "response_type", "code" },
                            { "scope", "openid email" },
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
                    JObject jResponse = JObject.Parse(json);
                    var requestUri = "";
                    if (jResponse.ContainsKey("request_uri"))
                    {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                        requestUri = jResponse.GetValue("request_uri").ToString();
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                    }
                    else
                    {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                        requestUri = jResponse.GetValue("error_description").ToString();
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                    }
                    res.Dispose();
                    return  String.Format(@"{0}?client_id={1}&response_type=code&request_uri={2}",
                        _appSettings.Value.authorisation_url,
                        _appSettings.Value.client_id,
                        requestUri);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                throw new NotImplementedException();
            }
        }



        public async Task<string> GetUsername(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Logout(string token)
        {
            throw new NotImplementedException();
        }
    }
}
