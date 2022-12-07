using System.Net.Http.Headers;
using System.Security.Authentication;
using Blazored.LocalStorage;
using CryptoWise.BlazorWasmClient.Authorization;
using CryptoWise.Shared.Exceptions;
using CryptoWise.Shared.MetaAuthAccount;
using Microsoft.AspNetCore.Components.Authorization;

namespace CryptoWise.BlazorWasmClient.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorage;
    
    private readonly HttpClient _client;

    public AuthenticationService(AuthenticationStateProvider authenticationStateProvider, ILocalStorageService localStorage, HttpClient client)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;
        _client = client;
    }
    
    public async Task<bool> Authenticate(GetJwtTokenResponse tokenResponse)
    {
        try
        {
            if (tokenResponse.Error)
                throw new CustomAuthenticationException(tokenResponse.Message);

            if (tokenResponse.JwtToken == string.Empty) return false;
            
            await _localStorage.SetItemAsync("token", tokenResponse.JwtToken);
                
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).SetUserAuthenticated(tokenResponse.JwtToken);
                
            _client.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("bearer", tokenResponse.JwtToken);
            
            return true;
        }
        catch(Exception e)
        {
            throw new CustomAuthenticationException("Error occured during authentication");
        }
    }
    
    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("token");
        ((CustomAuthenticationStateProvider)_authenticationStateProvider).SetUserLoggedOut();
        _client.DefaultRequestHeaders.Authorization = null;
    }
}