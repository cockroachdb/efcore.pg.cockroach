using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.CockroachDB.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.CockroachDB.Migrations;
using Npgsql.EntityFrameworkCore.CockroachDB.Storage.Internal;

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

        var serviceDescriptors = serviceCollection
            .Where(descriptor => descriptor.ServiceType == typeof(IRelationalTypeMappingSource) || 
                                 descriptor.ServiceType == typeof(IRelationalDatabaseCreator) ||
                                 descriptor.ServiceType == typeof(IProviderConventionSetBuilder) ||
                                 descriptor.ServiceType == typeof(IMigrationsSqlGenerator))
            .ToList();
        
        foreach (var descriptor in serviceDescriptors)
        {
            serviceCollection.Remove(descriptor);
        }
        
        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<IRelationalTypeMappingSource, CockroachTypeMappingSource>()
            .TryAdd<IMigrationsSqlGenerator, CockroachMigrationsSqlGenerator>()
            .TryAdd<IProviderConventionSetBuilder, CockroachConventionSetBuilder>()
            .TryAdd<IRelationalDatabaseCreator, CockroachDatabaseCreator>();
        
        return serviceCollection;
    }
}