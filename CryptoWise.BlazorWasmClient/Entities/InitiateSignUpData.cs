namespace CryptoWise.BlazorWasmClient.Entities;

public class InitiateSignUpData
{
    public string? AppName { get; set; } = null!;
    public List<string> RequiredUserData { get; set; } = new();
    public string UserIdentificator { get; set; } = null!;
}