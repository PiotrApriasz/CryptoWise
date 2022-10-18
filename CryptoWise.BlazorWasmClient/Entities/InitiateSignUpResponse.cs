using CryptoWise.Shared;

namespace CryptoWise.BlazorWasmClient.Entities;

public class InitiateSignUpResponse : BaseResponse
{
    public string RequestGuid { get; set; } = null!;
}