using ProjectAPI.Core;
using ProjectAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAPI.Middlewares
{
    public class JwtMiddleware
    {
        #region Fields

        private readonly ILogger<JwtMiddleware> _logger;
        private readonly RequestDelegate _requestDelegate;

        #endregion

        #region Constructors

        public JwtMiddleware(
            ILogger<JwtMiddleware> logger,
            RequestDelegate requestDelegate
            )
        {
            _logger = logger;
            _requestDelegate = requestDelegate;
        }

        #endregion

        #region Methods

        public async Task Invoke(HttpContext context, IAuthenticationService authenticationService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await AttachUserToContext(context, authenticationService, token);
            }
            await _requestDelegate(context);
        }

        #endregion

        #region Private methods

        private async Task AttachUserToContext(HttpContext context, IAuthenticationService authenticationService, string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(DataConfig.JwtOption.SigningKey);
                tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    context.Items["StatusToken"] = nameof(ConstantsMessage.TOKEN_INVALID);
                    context.Items["MessageToken"] = ConstantsMessage.TOKEN_INVALID;
                }
                else
                {
                    if (jwtToken.Claims.Count() > 0)
                    {
                        var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == "US")?.Value;

                        var account = await authenticationService.GetUserSyncAsync(userName);

                        if (account == null)
                        {
                            context.Items["StatusToken"] = nameof(ConstantsMessage.NODATA);
                            context.Items["MessageToken"] = ConstantsMessage.NODATA;
                        }
                        else if (accessToken != account.AccessToken)
                        {
                            context.Items["StatusToken"] = nameof(ConstantsMessage.UNAUTHORIZED);
                            context.Items["MessageToken"] = ConstantsMessage.UNAUTHORIZED;
                        }
                        else
                        {
                            context.Items["StatusToken"] = nameof(ConstantsMessage.AUTHORIZED);
                            context.Items["MessageToken"] = ConstantsMessage.AUTHORIZED;
                        }
                    }
                    else
                    {
                        context.Items["StatusToken"] = nameof(ConstantsMessage.UNAUTHORIZED);
                        context.Items["MessageToken"] = ConstantsMessage.UNAUTHORIZED;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("IDX10223"))
                {
                    context.Items["StatusToken"] = nameof(ConstantsMessage.TOKEN_EXPIRED);
                    context.Items["MessageToken"] = ConstantsMessage.TOKEN_EXPIRED;
                }
                else
                {
                    // do nothing if jwt validation fails
                    // user is not attached to context so request won't have access to secure routes
                    _logger.LogError(ex.ToString());

                    context.Items["StatusToken"] = nameof(ConstantsMessage.BADREQUEST);
                    context.Items["MessageToken"] = ConstantsMessage.BADREQUEST;
                }
            }
        }
        #endregion
    }
}