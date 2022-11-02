using CryptoWise.Shared;
using CryptoWise.Shared.MetaAuthAccount;

namespace CryptoWise.BlazorWasmClient.Services;

public interface IAccountService
{
    Task<InitiateSignUpCwResponse> InitiateSignUp();
}