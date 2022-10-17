using CryptoWise.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoWise.API.Helpers;

public class DataContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;

    private readonly IConfiguration _configuration;

    public DataContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(_configuration.GetConnectionString("CryptoWiseDb") ?? string.Empty);
    }
}