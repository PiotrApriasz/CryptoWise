﻿using System.Text.Json.Serialization;

namespace CryptoWise.Shared.Account;

public class AuthenticateResponse : BaseResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public bool IsVerified { get; set; }
    public string JwtToken { get; set; }
    
    [JsonIgnore] 
    public string RefreshToken { get; set; }
}