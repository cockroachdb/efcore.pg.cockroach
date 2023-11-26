using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Metadata.Conventions;

/// <summary>
/// 
/// </summary>
public class CockroachConventionSetBuilder : NpgsqlConventionSetBuilder
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly Version _postgresVersion;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="typeMappingSource"></param>
    /// <param name="npgsqlSingletonOptions"></param>
    public CockroachConventionSetBuilder(ProviderConventionSetBuilderDependencies dependencies, RelationalConventionSetBuilderDependencies relationalDependencies, IRelationalTypeMappingSource typeMappingSource, INpgsqlSingletonOptions npgsqlSingletonOptions) : base(dependencies, relationalDependencies, typeMappingSource, npgsqlSingletonOptions)
    {
        _typeMappingSource = typeMappingSource;
        _postgresVersion = npgsqlSingletonOptions.PostgresVersion;
    }

    /// <inheritdoc />
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        var valueGenerationStrategyConvention =
            new NpgsqlValueGenerationStrategyConvention(Dependencies, RelationalDependencies, _postgresVersion);
        conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
        conventionSet.ModelInitializedConventions.Add(new RelationalMaxIdentifierLengthConvention(63, Dependencies, RelationalDependencies));

        ValueGenerationConvention valueGenerationConvention = new NpgsqlValueGenerationConvention(Dependencies, RelationalDependencies);
        ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, valueGenerationConvention);

        ReplaceConvention(
            conventionSet.EntityTypeAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

        ReplaceConvention(conventionSet.EntityTypePrimaryKeyChangedConventions, valueGenerationConvention);

        ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGenerationConvention);

        ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGenerationConvention);

        var storeGenerationConvention =
            new NpgsqlStoreGenerationConvention(Dependencies, RelationalDependencies);
        ReplaceConvention(conventionSet.PropertyAnnotationChangedConventions, storeGenerationConvention);
        ReplaceConvention(
            conventionSet.PropertyAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

        conventionSet.ModelFinalizingConventions.Add(valueGenerationStrategyConvention);
        conventionSet.ModelFinalizingConventions.Add(new NpgsqlPostgresModelFinalizingConvention(_typeMappingSource));
        ReplaceConvention(conventionSet.ModelFinalizingConventions, storeGenerationConvention);
        ReplaceConvention(
            conventionSet.ModelFinalizingConventions,
            (SharedTableConvention)new NpgsqlSharedTableConvention(Dependencies, RelationalDependencies));

        ReplaceConvention(
            conventionSet.ModelFinalizedConventions,
            (RuntimeModelConvention)new NpgsqlRuntimeModelConvention(Dependencies, RelationalDependencies));

        return conventionSet;
    }
}