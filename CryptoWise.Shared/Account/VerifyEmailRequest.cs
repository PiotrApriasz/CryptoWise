using System.ComponentModel.DataAnnotations;

namespace CryptoWise.Shared.Account;

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; }
}