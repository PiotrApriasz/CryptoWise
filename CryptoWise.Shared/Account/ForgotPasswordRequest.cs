using System.ComponentModel.DataAnnotations;

namespace CryptoWise.Shared.Account;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}