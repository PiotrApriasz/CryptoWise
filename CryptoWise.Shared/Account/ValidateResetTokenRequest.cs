using System.ComponentModel.DataAnnotations;

namespace CryptoWise.Shared.Account;

public class ValidateResetTokenRequest
{
    [Required]
    public string Token { get; set; }
}