namespace CryptoWise.API.Services.Account;

public interface IEmailService
{
    void Send(string to, string subject, string html, string from = null);
}