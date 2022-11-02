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
            throw new MetaAuthException("There is no response from cryptowise API for Initiate Sign Up Action");
        
        if (result.Error)
            throw new MetaAuthException(result.Message);

        return result;
    }
}