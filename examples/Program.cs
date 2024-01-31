using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace CockroachEfCoreExample;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");
        if (connectionString == null)
        {
            var connStringBuilder = new NpgsqlConnectionStringBuilder();
            connStringBuilder.SslMode = SslMode.Prefer;
            connStringBuilder.Host = "localhost";
            connStringBuilder.Port = 26257;
            connStringBuilder.Username = "username";
            connStringBuilder.Password = "password";
            connStringBuilder.Database = "Bank";

            connectionString = connStringBuilder.ConnectionString;
        }

        await using var ctx = new BankContext(connectionString);
        await ctx.Database.EnsureCreatedAsync();

        // Clean-up Accounts table
        await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Accounts\"");

        var example = args.FirstOrDefault();
        if (example != "Transaction")
        {
            await SimpleExample.Run(connectionString);
        }
        else
        {
            await TransactionExample.Run(connectionString);
        }
    }
}