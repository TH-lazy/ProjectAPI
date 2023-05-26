using ProjectAPI.Middlewares;
using ProjectAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAPI.Services
{
    public interface IAuthenticationService
    {
        bool CheckToken_ExpiredDate(string accessToken, JwtOptions currentOptions);
        string GenerateToken(Dictionary<string, string> liststrClaim, JwtOptions currentOptions);
        Task<bool> UpdateUserSyncAsync(string userName, string accessToken);
        Task<bool> InsertUserSyncAsync(string userName, string accessToken);
        Task<SysUsersSync> GetUserSyncAsync(string userName);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly DataContext _context;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(DataContext context, ILogger<AuthenticationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool CheckToken_ExpiredDate(string accessToken, JwtOptions currentOptions)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(currentOptions.SigningKey);
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

                if (jwtToken.Claims.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return false;
        }

        public string GenerateToken(Dictionary<string, string> liststrClaim, JwtOptions currentOptions)
        {
            List<Claim> listClaim = new List<Claim>();
            liststrClaim.ToList().ForEach(x =>
            {
                listClaim.Add(new Claim(x.Key, x.Value));
            });

            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(currentOptions.SigningKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(listClaim),
                Expires = DateTime.Now.AddMinutes(currentOptions.ExpiredToken),
                SigningCredentials = signinCredentials
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> UpdateUserSyncAsync(string userName, string accessToken)
        {
            var accountExist = false;
            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@p_UserName", userName),
                    new SqlParameter("@p_AccessToken", accessToken)
                };

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "[dbo].[SYS_USERS_SYNC_Upd]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters);

                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }
                    using (var dtReader = await cmd.ExecuteReaderAsync())
                    {
                        if (dtReader.HasRows)
                        {
                            while (dtReader.Read())
                            {
                                return dtReader["Result"].ToString() == "0";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return accountExist;
        }

        public async Task<bool> InsertUserSyncAsync(string userName, string accessToken)
        {
            var accountExist = false;
            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@p_UserName", userName),
                    new SqlParameter("@p_AccessToken", accessToken)
                };

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "[dbo].[SYS_USERS_SYNC_Ins]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters);

                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }
                    using (var dtReader = await cmd.ExecuteReaderAsync())
                    {
                        if (dtReader.HasRows)
                        {
                            while (dtReader.Read())
                            {
                                return dtReader["Result"].ToString() == "0";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return accountExist;
        }

        public async Task<SysUsersSync> GetUserSyncAsync(string userName)
        {
            SysUsersSync accountExist = null;
            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@p_UserName", userName)
                };

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "[dbo].[SYS_USERS_SYNC_Get]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters);

                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }
                    using (var dtReader = await cmd.ExecuteReaderAsync())
                    {
                        if (dtReader.HasRows)
                        {
                            while (dtReader.Read())
                            {
                                accountExist = new SysUsersSync();
                                accountExist.UserName = dtReader["UserName"].ToString();
                                accountExist.AccessToken = dtReader["AccessToken"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return accountExist;
        }
    }
}