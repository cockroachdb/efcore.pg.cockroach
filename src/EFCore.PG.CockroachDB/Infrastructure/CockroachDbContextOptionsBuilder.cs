using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Infrastructure;

/// <summary>
/// 
/// </summary>
public class CockroachDbContextOptionsBuilder : NpgsqlDbContextOptionsBuilder
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionsBuilder"></param>
    public CockroachDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder) : base(optionsBuilder)
    {
    }
}