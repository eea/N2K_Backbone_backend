using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace N2K_BackboneBackEnd.Models {

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


    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromHeaderModelAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        public BindingSource BindingSource => BindingSource.Query;

        public string Name { get; set; }
    }

    public class BearerHeader
    {
        [FromHeader]
        [Required]
        public string Bearer { get; set; } = "";
    }

}
