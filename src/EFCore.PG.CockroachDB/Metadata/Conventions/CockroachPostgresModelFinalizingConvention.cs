using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Metadata.Conventions;

public class CockroachPostgresModelFinalizingConvention : NpgsqlPostgresModelFinalizingConvention
{
    public CockroachPostgresModelFinalizingConvention(IRelationalTypeMappingSource typeMappingSource) : base(typeMappingSource)
    {
    }
    
    /// <summary>
    ///     Detects properties which are uint, OnAddOrUpdate and configured as concurrency tokens, and maps these to the PostgreSQL
    ///     internal "xmin" column, which changes every time the row is modified.
    /// </summary>
    protected override void ProcessRowVersionProperty(IConventionProperty property, RelationalTypeMapping typeMapping)
    {
        //
    }
}