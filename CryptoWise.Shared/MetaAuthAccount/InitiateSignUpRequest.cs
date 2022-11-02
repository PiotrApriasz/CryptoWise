namespace CryptoWise.Shared.MetaAuthAccount;

public class InitiateSignUpRequest
{
    public string UserIdentificator { get; set; }
    public string AppName { get; set; }
    public List<string> RequiredUserData { get; set; }
    public string ReturnUrl { get; set; }
}