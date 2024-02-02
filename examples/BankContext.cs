using Microsoft.EntityFrameworkCore;

namespace CockroachEfCoreExample;

public class BankContext(string connectionString) : DbContext
{
    private string _connectionString = connectionString;

    public DbSet<Account> Accounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_connectionString);
}

public class Account
{
    public int Id { get; set; }
    public int Balance { get; set; }
}