namespace CryptoWise.BlazorWasmClient.Entities;

public class InitiateSignUpData
{
    public string? AppName { get; set; }
    public List<string> RequiredUserData { get; set; } = new();
    public string UserIdentificator { get; set; } = null!;
    public string? ReturnUrl { get; set; }
}