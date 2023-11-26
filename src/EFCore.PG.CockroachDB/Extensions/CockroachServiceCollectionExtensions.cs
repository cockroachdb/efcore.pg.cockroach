using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.CockroachDB.Migrations;

namespace System.Reflection;

/// <summary>
/// 
/// </summary>
public static class CockroachServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddEntityFrameworkCockroach(this IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<IMigrationsSqlGenerator, CockroachMigrationsSqlGenerator>();
        
        return serviceCollection;
    }
}