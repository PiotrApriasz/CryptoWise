using Microsoft.AspNetCore.Authorization;

namespace CryptoWise.BlazorWasmClient.Authentication;

public class Policies
{
    public const string IsUserLog = "IsUserLog";
    
    public static AuthorizationPolicy IsUserLogged()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
            .Build();
    }
}