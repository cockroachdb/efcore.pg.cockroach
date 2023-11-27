using Microsoft.EntityFrameworkCore.Design.Internal;
using Npgsql.EntityFrameworkCore.CockroachDB.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Design.Internal;

/// <summary>
/// 
/// </summary>
public class CockroachDesignTimeServices : NpgsqlDesignTimeServices
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceCollection"></param>
    public override void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        serviceCollection.AddEntityFrameworkNpgsql();
        serviceCollection.AddEntityFrameworkCockroach();
        
        new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
            .TryAdd<ICSharpRuntimeAnnotationCodeGenerator, NpgsqlCSharpRuntimeAnnotationCodeGenerator>()
            .TryAdd<IAnnotationCodeGenerator, NpgsqlAnnotationCodeGenerator>()
            .TryAdd<IDatabaseModelFactory, CockroachDatabaseModelFactory>()
            .TryAdd<IProviderConfigurationCodeGenerator, NpgsqlCodeGenerator>()
            .TryAddCoreServices();
    }
}