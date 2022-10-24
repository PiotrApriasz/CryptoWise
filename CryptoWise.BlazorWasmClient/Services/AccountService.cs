using System.Net.Http.Json;
using CryptoWise.BlazorWasmClient.Entities;
using CryptoWise.Shared;

namespace CryptoWise.BlazorWasmClient.Services;

public class AccountService : IAccountService
{
    private readonly HttpClient _client;
    private readonly string? _metaAuthBaseAddress;
    private readonly IConfiguration _configuration;

    public AccountService(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
        _metaAuthBaseAddress = configuration["MetaAuthApi:Url"];
    }

    public async Task<InitiateSignUpResponse> InitiateSignUp(InitiateSignUpData signUpData)
    {
        var appName = _configuration["MetaAuthApi:AppName"];
        var returlUrl = _configuration["MetaAuthApi:ReturnUrl"];
        var requiredUserData = _configuration.GetSection("MetaAuthApi:RequiredUserData").Get<List<string>>();

        var uri = $"{_metaAuthBaseAddress}signup";
        signUpData.AppName = appName;
        signUpData.RequiredUserData = requiredUserData;
        signUpData.ReturnUrl = returlUrl;

        var result = await _client.PostAsJsonAsync(uri, signUpData);

        var response = await result.Content.ReadFromJsonAsync<InitiateSignUpResponse>();
        return response!;
    }
}