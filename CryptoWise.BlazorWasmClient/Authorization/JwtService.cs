using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace CryptoWise.BlazorWasmClient.Authorization;

public class JwtService : IJwtService
{
    private readonly ILocalStorageService _localStorage;

    public JwtService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task AddJwtToken(HttpClient client)
    {
        if (await _localStorage.ContainKeyAsync("token"))
            client.DefaultRequestHeaders.Authorization 
                = new AuthenticationHeaderValue("Bearer", await _localStorage.GetItemAsync<string>("token"));
    }
}