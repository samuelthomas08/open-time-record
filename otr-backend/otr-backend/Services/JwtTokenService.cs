using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using otr_backend.Models;

namespace otr_backend.Services;

/// <summary>Issues signed JSON Web Tokens for authenticated users.</summary>
public class JwtTokenService
{
    /// <summary>How long an issued token stays valid.</summary>
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(7);

    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Creates a token containing the user's id, e-mail address and role names.
    /// Issuer, audience and signing key come from the <c>Jwt</c> configuration section.
    /// </summary>
    /// <param name="user">The user the token is issued for.</param>
    /// <param name="roles">Names of the roles to embed as role claims.</param>
    /// <returns>The serialized, signed token.</returns>
    public string GenerateToken(User user, IEnumerable<string> roles)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        ];
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        SigningCredentials credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(TokenLifetime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
