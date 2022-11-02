namespace CryptoWise.API.Helpers;

public class MetaAuth
{
    public string Url { get; set; }
    public string ClientUrl { get; set; }
    public string AppName { get; set; }
    public string ReturnUrl { get; set; }
    public List<string> RequiredUserData { get; set; }
}