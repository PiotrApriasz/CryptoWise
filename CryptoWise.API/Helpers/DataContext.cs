using CryptoWise.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoWise.API.Helpers;

public class DataContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }

    private readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(Configuration.GetConnectionString("CryptoWiseDb") ?? string.Empty);
    }
}