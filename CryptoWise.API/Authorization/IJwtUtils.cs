using CryptoWise.API.Entities;

namespace CryptoWise.API.Authorization;

public interface IJwtUtils
{
    public string GenerateJwtToken(Account account);
    public TokenValidationResult? ValidateJwtToken(string? token);
    public RefreshToken GenerateRefreshToken(string ipAddress);
}