using System.Net.Http.Json;
using CryptoWise.Shared;
using CryptoWise.Shared.Exceptions;
using CryptoWise.Shared.MetaAuthAccount;

namespace CryptoWise.BlazorWasmClient.Services;

public class AccountService : IAccountService
{
    private readonly HttpClient _client;
    private readonly string? _cryptowiseBaseAddress;
    private readonly IConfiguration _configuration;

    public AccountService(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
        _cryptowiseBaseAddress = configuration["CryptowiseAPI:Url"];
    }

    public async Task<InitiateSignUpCwResponse> InitiateSignUp()
    {
        var result = await _client
            .GetFromJsonAsync<InitiateSignUpCwResponse>($"{_cryptowiseBaseAddress}metaauth/initiateSignUp");

        if (result is null)
            throw new MetaAuthException("There is no response from cryptowise API for Initiate Sign Up action");
        
        if (result.Error)
            throw new MetaAuthException(result.Message);

        return result;
    }

    public async Task<InitiateSignInCwResponse> InitiateSignIn()
    {
        var response = await _client
            .GetAsync($"{_cryptowiseBaseAddress}metaauth/initiateSignIn");

        var result = await response.Content.ReadFromJsonAsync<InitiateSignInCwResponse>();
        
        if (result is null)
            throw new MetaAuthException("There is no response from cryptowise API for Initiate Sign In action");

        if (result.Error)
            throw new MetaAuthException(result.Message);

        return result;
    }

    public async Task<GetJwtTokenResponse> GetJwtToken(string requestId)
    {
        var response = await _client
            .GetAsync($"{_cryptowiseBaseAddress}metaauth/getAccessToken/{requestId}");
        

        var result = await response.Content.ReadFromJsonAsync<GetJwtTokenResponse>();
        
        if (result is null)
            throw new MetaAuthException("There is no response from cryptowise API for Get JWT token action");
        
        if (result.Error)
            throw new MetaAuthException(result.Message);

        return result;
    }
}