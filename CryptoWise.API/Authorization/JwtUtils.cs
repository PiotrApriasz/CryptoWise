using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CryptoWise.API.Entities;
using CryptoWise.API.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CryptoWise.API.Authorization;

public class JwtUtils : IJwtUtils
{
    private readonly DataContext _context;
    private readonly AppSettings _appSettings;

    public JwtUtils(DataContext context, IOptions<AppSettings> appSettings)
    {
        _context = context;
        _appSettings = appSettings.Value;
    }

    public string GenerateJwtToken(Account account)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", account.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Username.ToString()),
                new Claim(ClaimTypes.Name, account.Username.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public int? ValidateJwtToken(string? token)
    {
        if (token == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);
            
            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

            return accountId;
        }
        catch
        {
            return null;
        }
    }

    public RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var refreshToken = new RefreshToken()
        {
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        var tokenIsUniuque = !_context.Accounts.Any(a => a.RefreshTokens.Any(t => t.Token == refreshToken.Token));

        return !tokenIsUniuque ? GenerateRefreshToken(ipAddress) : refreshToken;
    }
}