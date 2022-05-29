using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using CryptoWise.API.Authorization;
using CryptoWise.API.Entities;
using CryptoWise.API.Exceptions;
using CryptoWise.API.Helpers;
using CryptoWise.Shared.Account;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CryptoWise.API.Services.Account;

public class AccountService : IAccountService
{
    private readonly DataContext _context;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    private readonly IEmailService _emailService;
    
    public AccountService(DataContext context, IJwtUtils jwtUtils, IMapper mapper,
        IOptions<AppSettings> appSettings, IEmailService emailService)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _emailService = emailService;
    }

    public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
    {
        var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

        if (account == null || !account.IsVerified || !BCrypt.Net.BCrypt.Verify(model.Password, account.PasswordHash))
            throw new AppException("Email or password is incorrect");

        var jwtToken = _jwtUtils.GenerateJwtToken(account);
        var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        account.RefreshTokens.Add(refreshToken);
        
        RemoveOldRefreshTokens(account);

        _context.Update(account);
        _context.SaveChanges();

        var response = _mapper.Map<AuthenticateResponse>(account);
        response.JwtToken = jwtToken;
        response.RefreshToken = refreshToken.Token;
        return response;
    }

    public AuthenticateResponse RefreshToken(string token, string ipAddress)
    {
        var account = GetAccountByRefreshToken(token);
        var refreshToken = account.RefreshTokens.Single(x => x.Token == token);
        
        if (refreshToken.IsRevoked)
        {
            RevokeDescendantRefreshTokens(refreshToken, account, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
            _context.Update(account);
            _context.SaveChanges();
        }
        
        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");
        
        var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
        account.RefreshTokens.Add(newRefreshToken);
        
        RemoveOldRefreshTokens(account);
        
        _context.Update(account);
        _context.SaveChanges();
        
        var jwtToken = _jwtUtils.GenerateJwtToken(account);
        
        var response = _mapper.Map<AuthenticateResponse>(account);
        response.JwtToken = jwtToken;
        response.RefreshToken = newRefreshToken.Token;
        return response;
    }

    public void RevokeToken(string token, string ipAddress)
    {
        var account = GetAccountByRefreshToken(token);
        var refreshToken = account.RefreshTokens.Single(x => x.Token == token);
        
        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");
        
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        _context.Update(account);
        _context.SaveChanges();
    }

    public void Register(RegisterRequest model, string origin)
    {
        if (_context.Accounts.Any(x => x.Email == model.Email))
        {
            SendAlreadyRegisteredEmail(model.Email, origin);
            return;
        }
        
        var account = _mapper.Map<Entities.Account>(model);
        
        var isFirstAccount = !_context.Accounts.Any();
        account.Role = isFirstAccount ? Role.Admin : Role.User;
        account.Created = DateTime.UtcNow;
        account.VerificationToken = GenerateVerificationToken();
        
        account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        
        _context.Accounts.Add(account);
        _context.SaveChanges();
        
        SendVerificationEmail(account, origin);
    }

    public void VerifyEmail(string token)
    {
        var account = _context.Accounts.SingleOrDefault(x => x.VerificationToken == token);
        
        if (account == null) 
            throw new AppException("Verification failed");
        
        account.Verified = DateTime.UtcNow;
        account.VerificationToken = null;
        
        _context.Accounts.Update(account);
        _context.SaveChanges();
    }

    public void ForgotPassword(ForgotPasswordRequest model, string origin)
    {
        var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);
        
        if (account == null) return;
        
        account.ResetToken = GenerateResetToken();
        account.ResetTokenExpires = DateTime.UtcNow.AddDays(1);
        
        _context.Accounts.Update(account);
        _context.SaveChanges();
        
        SendPasswordResetEmail(account, origin);
    }
    
    public void ValidateResetToken(ValidateResetTokenRequest model)
    {
        GetAccountByResetToken(model.Token);
    }

    public void ResetPassword(ResetPasswordRequest model)
    {
        var account = GetAccountByResetToken(model.Token);
        
        account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        account.PasswordReset = DateTime.UtcNow;
        account.ResetToken = null;
        account.ResetTokenExpires = null;

        _context.Accounts.Update(account);
        _context.SaveChanges();
    }
    
    public AccountResponse GetById(int id)
    {
        var account = GetAccount(id);
        return _mapper.Map<AccountResponse>(account);
    }
    
    public IEnumerable<AccountResponse> GetAll()
    {
        var accounts = _context.Accounts;
        return _mapper.Map<IList<AccountResponse>>(accounts);
    }
    
    public AccountResponse Create(CreateRequest model)
    {
        if (_context.Accounts.Any(x => x.Email == model.Email))
            throw new AppException($"Email '{model.Email}' is already registered");
        
        var account = _mapper.Map<Entities.Account>(model);
        account.Created = DateTime.UtcNow;
        account.Verified = DateTime.UtcNow;
        
        account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        
        _context.Accounts.Add(account);
        _context.SaveChanges();

        return _mapper.Map<AccountResponse>(account);
    }
    
    public AccountResponse Update(int id, UpdateRequest model)
    {
        var account = GetAccount(id);
        
        if (account.Email != model.Email && _context.Accounts.Any(x => x.Email == model.Email))
            throw new AppException($"Email '{model.Email}' is already registered");
        
        if (!string.IsNullOrEmpty(model.Password))
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        
        _mapper.Map(model, account);
        account.Updated = DateTime.UtcNow;
        _context.Accounts.Update(account);
        _context.SaveChanges();

        return _mapper.Map<AccountResponse>(account);
    }
    
    public void Delete(int id)
    {
        var account = GetAccount(id);
        _context.Accounts.Remove(account);
        _context.SaveChanges();
    }

    private Entities.Account GetAccount(int id)
    {
        var account = _context.Accounts.Find(id);
        if (account == null) throw new KeyNotFoundException("Account not found");
        return account;
    }
    
    private Entities.Account GetAccountByRefreshToken(string token)
    {
        var account = _context.Accounts.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
        if (account == null) throw new AppException("Invalid token");
        return account;
    }
    
    private Entities.Account GetAccountByResetToken(string token)
    {
        var account = _context.Accounts.SingleOrDefault(x =>
            x.ResetToken == token && x.ResetTokenExpires > DateTime.UtcNow);
        if (account == null) throw new AppException("Invalid token");
        return account;
    }
    
    private string GenerateJwtToken(Entities.Account account)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", account.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Username.ToString()),
                new Claim(ClaimTypes.Name, account.Username.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    private string GenerateResetToken()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        
        var tokenIsUnique = !_context.Accounts.Any(x => x.ResetToken == token);
        if (!tokenIsUnique)
            return GenerateResetToken();
        
        return token;
    }
    
    private string GenerateVerificationToken()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        
        var tokenIsUnique = !_context.Accounts.Any(x => x.VerificationToken == token);
        if (!tokenIsUnique)
            return GenerateVerificationToken();
        
        return token;
    }
    
    private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void RemoveOldRefreshTokens(Entities.Account account)
    {
        account.RefreshTokens.RemoveAll(x => 
            !x.IsActive && 
            x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
    }
    
    private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, Entities.Account account, string ipAddress, string reason)
    {
        if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        {
            var childToken = account.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
            if (childToken.IsActive)
                RevokeRefreshToken(childToken, ipAddress, reason);
            else
                RevokeDescendantRefreshTokens(childToken, account, ipAddress, reason);
        }
    }
    
    private void RevokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
    }
    
    private void SendVerificationEmail(Entities.Account account, string origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
        {
            var verifyUrl = $"{origin}/account/verify-email?token={account.VerificationToken}";
            message = $@"<p>Please click the below link to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
        }
        else
        {
            message = $@"<p>Please use the below token to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
                            <p><code>{account.VerificationToken}</code></p>";
        }

        _emailService.Send(
            to: account.Email,
            subject: "Sign-up Verification API - Verify Email",
            html: $@"<h4>Verify Email</h4>
                        <p>Thanks for registering!</p>
                        {message}"
        );
    }
    
    private void SendAlreadyRegisteredEmail(string email, string origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
            message = $@"<p>If you don't know your password please visit the <a href=""{origin}/account/forgot-password"">forgot password</a> page.</p>";
        else
            message = "<p>If you don't know your password you can reset it via the <code>/accounts/forgot-password</code> api route.</p>";

        _emailService.Send(
            to: email,
            subject: "Sign-up Verification API - Email Already Registered",
            html: $@"<h4>Email Already Registered</h4>
                        <p>Your email <strong>{email}</strong> is already registered.</p>
                        {message}"
        );
    }
    
    private void SendPasswordResetEmail(Entities.Account account, string origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
        {
            var resetUrl = $"{origin}/account/reset-password?token={account.ResetToken}";
            message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
        }
        else
        {
            message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                            <p><code>{account.ResetToken}</code></p>";
        }

        _emailService.Send(
            to: account.Email,
            subject: "Sign-up Verification API - Reset Password",
            html: $@"<h4>Reset Password Email</h4>
                        {message}"
        );
    }
}