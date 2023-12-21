using System.Data.Common;
using Npgsql.EntityFrameworkCore.CockroachDB.Infrastructure;
using Npgsql.EntityFrameworkCore.CockroachDB.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace System.Reflection;

/// <summary>
/// 
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionsBuilder"></param>
    /// <returns></returns>
    public static DbContextOptionsBuilder UseCockroach(this DbContextOptionsBuilder optionsBuilder)
    {
        var extension = new CockroachOptionsExtension();
        
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        
        return optionsBuilder;
    }
}