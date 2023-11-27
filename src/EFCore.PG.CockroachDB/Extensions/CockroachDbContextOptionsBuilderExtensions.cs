using Npgsql.EntityFrameworkCore.CockroachDB.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace System.Reflection;

/// <summary>
/// 
/// </summary>
public static class CockroachDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionsBuilder"></param>
    /// <param name="connectionString"></param>
    /// <param name="npgsqlOptionsAction"></param>
    /// <returns></returns>
    public static DbContextOptionsBuilder UseCockroach(
        this DbContextOptionsBuilder optionsBuilder,
        string? connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));

        var extension = (CockroachOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        npgsqlOptionsAction?.Invoke(new NpgsqlDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }
    
    private static CockroachOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<CockroachOptionsExtension>() is { } existing
            ? new CockroachOptionsExtension(existing)
            : new CockroachOptionsExtension();
}