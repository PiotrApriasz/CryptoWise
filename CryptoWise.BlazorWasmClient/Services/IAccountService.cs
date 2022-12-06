using CryptoWise.Shared;
using CryptoWise.Shared.MetaAuthAccount;

namespace CryptoWise.BlazorWasmClient.Services;

public interface IAccountService
{
    Task<InitiateSignUpCwResponse> InitiateSignUp();
    Task<InitiateSignInCwResponse> InitiateSignIn();
    Task<GetJwtTokenResponse> GetJwtToken(string requestId);
}