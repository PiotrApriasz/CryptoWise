using CryptoWise.Shared.MetaAuthAccount;

namespace CryptoWise.BlazorWasmClient.Authentication;

public interface IAuthenticationService
{
    Task<bool> Authenticate(GetJwtTokenResponse tokenResponse);
    Task Logout();
}