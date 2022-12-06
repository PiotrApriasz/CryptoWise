namespace CryptoWise.Shared.MetaAuthAccount;

public class InitiateSignInRequest
{
    public string AppName { get; set; }
    public string FailReturnUrl { get; set; }
    public string SuccessReturnUrl { get; set; }
}