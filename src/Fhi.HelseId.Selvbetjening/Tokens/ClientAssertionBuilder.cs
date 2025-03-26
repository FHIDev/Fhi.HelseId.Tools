using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fhi.IdentityModel.Tokens
{
    /// <summary>
    /// TODO: Replace with potentially new authentication package.
    /// </summary>
    internal static class ClientAssertionBuilder
    {
        public static string CreateClientAssertionJwt(string issuer, string clientId, string privateKey)
        {
            var securityKey = new JsonWebKey(privateKey);
            string token = CreateClientAssertionJwt(issuer, clientId, securityKey);

            return token;
        }

        public static string CreateClientAssertionJwt(string issuer, string clientId, JsonWebKey securityKey)
        {
            //Create payload
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, clientId),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            };
            var payload = new JwtPayload(clientId, issuer, claims, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(60));

            //Create header
            if (string.IsNullOrEmpty(securityKey.Alg))
                securityKey.Alg = SecurityAlgorithms.RsaSha256;
            var signingCredentials = new SigningCredentials(securityKey, securityKey.Alg);
            var header = new JwtHeader(signingCredentials, null, "client-authentication+jwt");

            //create signed JWT with header and payload 
            var jwtSecurityToken = new JwtSecurityToken(header, payload);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return token;
        }
    }
}
