using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CockroachEfCoreExample;

/// <summary>
/// 
/// </summary>
public static class SimpleExample
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionString"></param>
    public static async Task Run(string connectionString)
    {
        await using var ctx = new BankContext(connectionString);
        
        // Insert two rows into the Accounts table.
        ctx.Accounts.AddRange(new []
        {
            new Account() { Id = 1, Balance = 1000 },
            new Account() { Id = 2, Balance = 250 }
        });
        await ctx.SaveChangesAsync();

        // Print out the balances.
        Console.WriteLine("Initial balances:");

        var accounts = await ctx.Accounts.ToListAsync();
        foreach (var account in accounts)
        {
            Console.Write("\taccount {0}: {1}\n", account.Id, account.Balance);
        }
        
        // Update the balance for the first account
        var firstAccount = accounts[0];
        firstAccount.Balance = 1500;
        await ctx.SaveChangesAsync();
        
        // Add a new account
        ctx.Accounts.Add(new Account()
        {
            Id = 3, Balance = 5000 
        });
        await ctx.SaveChangesAsync();

        // Find and print out the balance for all accounts whose balance is greater than or equal to 1500
        Console.WriteLine("Final balances:");
        
        var largeAccounts = await ctx.Accounts.Where(a => a.Balance >= 1500).ToListAsync();
        foreach (var account in largeAccounts)
        {
            Console.Write("\taccount {0}: {1}\n", account.Id, account.Balance);
        }
    }
}