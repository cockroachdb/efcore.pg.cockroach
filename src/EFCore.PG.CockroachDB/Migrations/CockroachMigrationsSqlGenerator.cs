using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Migrations;

public class CockroachMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
{
    private readonly RelationalTypeMapping _stringTypeMapping;
    
    /// <summary>
    /// The backend version to target.
    /// </summary>
    private readonly Version _postgresVersion;
    
    public CockroachMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, INpgsqlSingletonOptions npgsqlSingletonOptions) : base(dependencies, npgsqlSingletonOptions)
    {
        _postgresVersion = npgsqlSingletonOptions.PostgresVersion;
        _stringTypeMapping = dependencies.TypeMappingSource.GetMapping(typeof(string))
                             ?? throw new InvalidOperationException("No string type mapping found");
    }

    #region Storage parameter utilities

    private Dictionary<string, string> GetStorageParameters(Annotatable annotatable)
        => annotatable.GetAnnotations()
            .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix, StringComparison.Ordinal))
            .ToDictionary(
                a => a.Name.Substring(NpgsqlAnnotationNames.StorageParameterPrefix.Length),
                a => GenerateStorageParameterValue(a.Value!)
            );

    private static string GenerateStorageParameterValue(object value)
    {
        if (value is bool)
        {
            return (bool)value ? "true" : "false";
        }

        if (value is string)
        {
            return $"'{value}'";
        }

        return value.ToString()!;
    }

    #endregion Storage parameter utilities
    
    #region Helpers

    private string DelimitIdentifier(string identifier) =>
        Dependencies.SqlGenerationHelper.DelimitIdentifier(identifier);

    private string DelimitIdentifier(string name, string? schema) =>
        Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema);
    
    #endregion
    
    #region System column utilities

    private bool IsSystemColumn(string name)
        => name == "oid" && SystemColumnNames.Contains(name);
    
    /// <summary>
    /// Tables in PostgreSQL implicitly have a set of system columns, which are always there.
    /// We want to allow users to access these columns (i.e. xmin for optimistic concurrency) but
    /// they should never generate migration operations.
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/ddl-system-columns.html
    /// </remarks>
    private static readonly string[] SystemColumnNames = { "tableoid", "xmin", "cmin", "xmax", "cmax", "ctid" };
    
    #endregion System column utilities
    
    /// <summary>
    ///     Builds commands for the given <see cref="T:Microsoft.EntityFrameworkCore.Migrations.Operations.DropForeignKeyOperation" /> by making calls on the given
    ///     <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(DropForeignKeyOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" DROP CONSTRAINT ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder, true);
        }
    }
    
    /// <inheritdoc />
    protected override void Generate(DropCheckConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" DROP CONSTRAINT ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder, suppressTransaction: true);
    }
    
    /// <inheritdoc />
    protected override void Generate(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (!terminate && operation.Comment is not null)
        {
            throw new ArgumentException($"When generating migrations SQL for {nameof(CreateTableOperation)}, can't produce unterminated SQL with comments");
        }

        operation.Columns.RemoveAll(c => IsSystemColumn(c.Name));

        builder.Append("CREATE ");

        if (operation[NpgsqlAnnotationNames.UnloggedTable] is true)
        {
            builder.Append("UNLOGGED ");
        }

        builder
            .Append("TABLE ")
            .Append(DelimitIdentifier(operation.Name, operation.Schema))
            .AppendLine(" (");

        using (builder.Indent())
        {
            base.CreateTableColumns(operation, model, builder);
            base.CreateTableConstraints(operation, model, builder);
            builder.AppendLine();
        }

        builder.Append(")");

        // CockroachDB "interleave in parent" (https://www.cockroachlabs.com/docs/stable/interleave-in-parent.html)
        if (operation[CockroachDbAnnotationNames.InterleaveInParent] is string)
        {
            var interleaveInParent = new CockroachDbInterleaveInParent(operation);
            var parentTableSchema = interleaveInParent.ParentTableSchema;
            var parentTableName = interleaveInParent.ParentTableName;
            var interleavePrefix = interleaveInParent.InterleavePrefix;

            builder
                .AppendLine()
                .Append("INTERLEAVE IN PARENT ")
                .Append(DelimitIdentifier(parentTableName, parentTableSchema))
                .Append(" (")
                .Append(string.Join(", ", interleavePrefix.Select(c => DelimitIdentifier(c))))
                .Append(")");
        }

        var storageParameters = GetStorageParameters(operation);
        if (storageParameters.Count > 0)
        {
            builder
                .AppendLine()
                .Append("WITH (")
                .Append(string.Join(", ", storageParameters.Select(p => $"{p.Key}={p.Value}")))
                .Append(")");
        }

        // Comment on the table
        if (operation.Comment is not null)
        {
            builder.AppendLine(";");

            builder
                .Append("COMMENT ON TABLE ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" IS ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(operation.Comment));
        }

        // Comments on the columns
        foreach (var columnOp in operation.Columns.Where(c => c.Comment is not null))
        {
            var columnComment = columnOp.Comment;
            builder.AppendLine(";");

            builder
                .Append("COMMENT ON COLUMN ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema))
                .Append(".")
                .Append(DelimitIdentifier(columnOp.Name))
                .Append(" IS ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(columnComment));
        }

        if (terminate)
        {
            builder.AppendLine(";");
            EndStatement(builder, suppressTransaction: true);
        }
    }
    
    /// <inheritdoc />
    protected override void Generate(AlterTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        var madeChanges = false;

        // Storage parameters
        var oldStorageParameters = GetStorageParameters(operation.OldTable);
        var newStorageParameters = GetStorageParameters(operation);

        var newOrChanged = newStorageParameters.Where(p =>
            !oldStorageParameters.ContainsKey(p.Key) ||
            oldStorageParameters[p.Key] != p.Value
        ).ToList();

        if (newOrChanged.Count > 0)
        {
            builder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema));

            builder
                .Append(" SET (")
                .Append(string.Join(", ", newOrChanged.Select(p => $"{p.Key}={p.Value}")))
                .Append(")");

            builder.AppendLine(";");
            madeChanges = true;
        }

        var removed = oldStorageParameters
            .Select(p => p.Key)
            .Where(pn => !newStorageParameters.ContainsKey(pn))
            .ToList();

        if (removed.Count > 0)
        {
            builder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema));

            builder
                .Append(" RESET (")
                .Append(string.Join(", ", removed))
                .Append(")");

            builder.AppendLine(";");
            madeChanges = true;
        }

        // Comment
        if (operation.Comment != operation.OldTable.Comment)
        {
            builder
                .Append("COMMENT ON TABLE ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" IS ")
                .Append(_stringTypeMapping.GenerateSqlLiteral(operation.Comment));

            builder.AppendLine(";");
            madeChanges = true;
        }

        // Unlogged table (null is equivalent to false)
        var oldUnlogged = operation.OldTable[NpgsqlAnnotationNames.UnloggedTable] is true;
        var newUnlogged = operation[NpgsqlAnnotationNames.UnloggedTable] is true;

        if (oldUnlogged != newUnlogged)
        {
            builder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" SET ")
                .Append(newUnlogged ? "UNLOGGED" : "LOGGED")
                .AppendLine(";");

            madeChanges = true;
        }

        if (madeChanges)
        {
            EndStatement(builder, suppressTransaction: true);
        }
    }
    
    /// <inheritdoc />
    protected override void Generate(
        DropColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        // Never touch system columns
        if (IsSystemColumn(operation.Name))
        {
            return;
        }

        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" DROP COLUMN ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder, suppressTransaction: true);
        }
    }
    
    /// <inheritdoc />
    protected override void Generate(EnsureSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        if (operation.Name == "public")
        {
            return;
        }

        // PostgreSQL has CREATE SCHEMA IF NOT EXISTS, but that requires CREATE privileges on the database even if the schema already
        // exists. This blocks multi-tenant scenarios where the user has no database privileges.
        // So we procedurally check if the schema exists instead, and create it if not.
        var schemaName = operation.Name.Replace("'", "''");

        // CockroachDB doesn't yet support IF statement, https://github.com/cockroachdb/cockroach/issues/110080
        builder.AppendLine($"CREATE SCHEMA IF NOT EXISTS {DelimitIdentifier(operation.Name)};");

        EndStatement(builder);
    }
    
    /// <inheritdoc />
    public virtual void Generate(NpgsqlDropDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        var dbName = DelimitIdentifier(operation.Name);

        builder
            .AppendLine($"REVOKE CONNECT ON DATABASE {dbName} FROM PUBLIC;")
            .EndCommand(suppressTransaction: true)
            .AppendLine($"DROP DATABASE {dbName};");
        
        EndStatement(builder, suppressTransaction: true);
    }
    
    /// <inheritdoc />
    protected override void Generate(AddPrimaryKeyOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ADD ");
        PrimaryKeyConstraint(operation, model, builder);

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder, suppressTransaction: true);
        }
    }

    /// <inheritdoc />
    protected override void Generate(DropUniqueConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        builder
            // .Append("ALTER TABLE ")
            // .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append("DROP INDEX ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(" CASCADE")
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder, suppressTransaction: true);
    }
    
    /// <summary>
    ///     Generates a SQL fragment for a computed column definition for the given column metadata.
    /// </summary>
    /// <param name="schema"> The schema that contains the table, or <c>null</c> to use the default schema. </param>
    /// <param name="table"> The table that contains the column. </param>
    /// <param name="name"> The column name. </param>
    /// <param name="operation"> The column metadata. </param>
    /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
    /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
    protected override void ComputedColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        if (_postgresVersion < new Version(12, 0))
        {
            throw new NotSupportedException("Computed/generated columns aren't supported in PostgreSQL prior to version 12");
        }

        if (operation.IsStored != true)
        {
            throw new NotSupportedException(
                "Generated columns currently must be stored, specify 'stored: true' in " +
                $"'{nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql)}' in your context's OnModelCreating.");
        }

        var columnType = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model)!;
        builder
            .Append(DelimitIdentifier(name))
            .Append(" ")
            .Append(columnType);

        if (operation.Collation is not null)
        {
            builder
                .Append(" COLLATE ")
                .Append(DelimitIdentifier(operation.Collation));
        }

        var computedColumnSql = $"({operation.ComputedColumnSql})::{columnType}";

        builder
            .Append(" GENERATED ALWAYS AS (")
            .Append(computedColumnSql!)
            .Append(") STORED");

        if (!operation.IsNullable)
        {
            builder.Append(" NOT NULL");
        }
    }
}