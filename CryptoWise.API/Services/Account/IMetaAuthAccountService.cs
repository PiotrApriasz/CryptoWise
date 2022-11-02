using CryptoWise.Shared.MetaAuthAccount;

namespace CryptoWise.API.Services.Account;

public interface IMetaAuthAccountService
{
    void Authenticate();
    Task<InitiateSignUpCwResponse> InitiateSignUp();
}