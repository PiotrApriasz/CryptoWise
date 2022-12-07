using CryptoWise.API.Entities;

namespace CryptoWise.API.Authorization;

public class TokenValidationResult
{
    public string? Id { get; set; }
    public Account? Account { get; set; }
}