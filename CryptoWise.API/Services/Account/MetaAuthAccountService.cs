using CryptoWise.API.Helpers;
using CryptoWise.Shared.Exceptions;
using CryptoWise.Shared.MetaAuthAccount;
using Microsoft.Extensions.Options;

namespace CryptoWise.API.Services.Account;

public class MetaAuthAccountService : IMetaAuthAccountService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MetaAuth _metaAuth;

    public MetaAuthAccountService(IOptions<MetaAuth> metaAuthApi, IHttpClientFactory httpClientFactory)
    {
        _metaAuth = metaAuthApi.Value;
        _httpClientFactory = httpClientFactory;
    }

    public void Authenticate()
    {
        
    }

    public async Task<InitiateSignUpCwResponse> InitiateSignUp()
    {
        var userIdentificator = Guid.NewGuid().ToString();

        var initiateSignUpData = new InitiateSignUpRequest()
        {
            AppName = _metaAuth.AppName,
            ReturnUrl = _metaAuth.ReturnUrl,
            RequiredUserData = _metaAuth.RequiredUserData,
            UserIdentificator = userIdentificator
        };

        var httpClient = _httpClientFactory.CreateClient("MetaAuthClient");
        
        var result = await httpClient.PostAsJsonAsync("/signup", initiateSignUpData);
        
        var response = await result.Content.ReadFromJsonAsync<InitiateSignUpResponse>();

        if (response is null)
        {
            throw new MetaAuthException("There is no response from MetaAuth API for Initiate Sign Up Action");
        }

        var cwResponse = new InitiateSignUpCwResponse
        {
            Error = response.Error,
            Message = response.Message,
            FullSignUpAddress = $"{_metaAuth.ClientUrl}signup/{response.RequestGuid}"
        };

        return cwResponse;
    }
}