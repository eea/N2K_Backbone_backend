using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace N2K_BackboneBackEnd.Helpers
{
    public class ValidateHashAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {

    }

    public class ValidateHashAuthenticationHandler : AuthenticationHandler<ValidateHashAuthenticationSchemeOptions>
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

            //return Task.FromResult(AuthenticateResult.Fail("TokenParseException"));
        }

    }
}
