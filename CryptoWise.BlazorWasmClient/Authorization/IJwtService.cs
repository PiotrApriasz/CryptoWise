namespace CryptoWise.BlazorWasmClient.Authorization;

public interface IJwtService
{
    Task AddJwtToken(HttpClient client);
}