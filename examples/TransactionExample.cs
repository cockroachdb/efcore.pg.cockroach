using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace CockroachEfCoreExample;

/// <summary>
/// 
/// </summary>
public static class TransactionExample
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
        
        try
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync();
            await transaction.CreateSavepointAsync("cockroach_restart");
            while (true)
            {
                try
                {
                    await TransferFunds(ctx, transaction, 1, 2, 100);
                    await transaction.CommitAsync();
                    break;
                }
                catch (Exception exception)
                {
                    if (IsSerializationFailure(exception))
                    {
                        await transaction.RollbackToSavepointAsync("cockroach_restart");
                    }
                    else
                    {
                        throw;    
                    }
                }
            }
        }
        catch (DataException e)
        {
            Console.WriteLine(e.Message);
        }
        
        // Now printout the results.
        Console.WriteLine("Final balances:");
        
        foreach (var account in await ctx.Accounts.ToListAsync())
        {
            Console.Write("\taccount {0}: {1}\n", account.Id, account.Balance);
        }
    }
    
    private static async Task TransferFunds(BankContext ctx, IDbContextTransaction transaction, int from, int to,
        int amount)
    {
        int balance = 0;
        var fromAccount = await ctx.Accounts.Where(a => a.Id == from).FirstOrDefaultAsync();
        if (fromAccount == null)
        {
            throw new DataException($"Account id={from} not found");
        }
        
        var toAccount = await ctx.Accounts.Where(a => a.Id == to).FirstOrDefaultAsync();
        if (toAccount == null)
        {
            throw new DataException($"Account id={to} not found");
        }

        balance = fromAccount.Balance;
        if (balance < amount)
        {
            throw new DataException($"Insufficient balance in account id={from}");
        }

        fromAccount.Balance = fromAccount.Balance - amount;
        toAccount.Balance = toAccount.Balance + amount;

        await ctx.SaveChangesAsync();
    }

    private static bool IsSerializationFailure(Exception exception)
    {
        return exception is PostgresException { SqlState: "40001" } ||
               exception.InnerException is NpgsqlException { SqlState: "SqlState" };
    }
}