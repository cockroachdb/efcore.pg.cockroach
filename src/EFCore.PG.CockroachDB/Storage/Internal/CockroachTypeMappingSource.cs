using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Storage.Internal;

/// <summary>
/// 
/// </summary>
public class CockroachTypeMappingSource : NpgsqlTypeMappingSource
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="sqlGenerationHelper"></param>
    /// <param name="options"></param>
    public CockroachTypeMappingSource(
        TypeMappingSourceDependencies dependencies, 
        RelationalTypeMappingSourceDependencies relationalDependencies, 
        ISqlGenerationHelper sqlGenerationHelper, 
        INpgsqlSingletonOptions options) : base(dependencies, relationalDependencies, sqlGenerationHelper, options)
    {
        StoreTypeMappings["json"] = StoreTypeMappings["jsonb"];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mappingInfo"></param>
    /// <returns></returns>
    protected override RelationalTypeMapping? FindBaseMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;

        if (storeTypeName is "jsonb" or "json")
        {
            if (StoreTypeMappings.TryGetValue("jsonb", out var mappings))
            {
                if (clrType is null)
                {
                    return mappings[0];
                }
                    
                return new NpgsqlJsonTypeMapping("jsonb", clrType);
            }
        }

        if (mappingInfo.IsRowVersion == true)
        {
            return null;
        }

        return base.FindBaseMapping(in mappingInfo);
    }
}