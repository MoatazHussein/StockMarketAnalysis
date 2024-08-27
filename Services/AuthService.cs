using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using StockMarket.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StockMarket.Services
{

    public class AuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService( IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> GenerateUserJwtToken(UserLoginRequest user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_Key") ?? "JWT_Key"));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer : Environment.GetEnvironmentVariable("JWT_Issuer"),
                audience : Environment.GetEnvironmentVariable("JWT_Audience"),
                claims: claims,
                expires: DateTime.Now.AddYears(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateGeneralJwtToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_Key") ?? "JWT_Key"));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JWT_Issuer"),
                audience: Environment.GetEnvironmentVariable("JWT_Audience"),
                expires: DateTime.Now.AddYears(1), 
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
