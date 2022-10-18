using CryptoWise.BlazorWasmClient.Entities;
using CryptoWise.Shared;

namespace CryptoWise.BlazorWasmClient.Services;

public interface IAccountService
{
    Task<InitiateSignUpResponse> InitiateSignUp(InitiateSignUpData signUpData);
}