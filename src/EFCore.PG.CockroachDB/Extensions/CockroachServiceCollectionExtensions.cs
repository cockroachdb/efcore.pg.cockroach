using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.CockroachDB.Migrations;

namespace System.Reflection;

public static class CockroachServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkCockroach(this IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<IMigrationsSqlGenerator, CockroachMigrationsSqlGenerator>();
        
        return serviceCollection;
    }
}