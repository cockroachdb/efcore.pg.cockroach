﻿using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlTestStore : RelationalTestStore
{
    private readonly string _scriptPath;
    private readonly string _additionalSql;

    private const string Northwind = "Northwind";

    public const int CommandTimeout = 600;

    public static readonly string NorthwindConnectionString = CreateConnectionString(Northwind);

    public static NpgsqlTestStore GetNorthwindStore()
        => (NpgsqlTestStore)NpgsqlNorthwindTestStoreFactory.Instance
            .GetOrCreate(NpgsqlNorthwindTestStoreFactory.Name).Initialize(null, (Func<DbContext>)null);

    // ReSharper disable once UnusedMember.Global
    public static NpgsqlTestStore GetOrCreateInitialized(string name)
        => new NpgsqlTestStore(name).InitializeNpgsql(null, (Func<DbContext>)null, null);

    public static NpgsqlTestStore GetOrCreate(
        string name,
        string scriptPath = null,
        string additionalSql = null,
        string connectionStringOptions = null)
        => new(name, scriptPath, additionalSql, connectionStringOptions);

    public static NpgsqlTestStore Create(string name, string connectionStringOptions = null)
        => new(name, connectionStringOptions: connectionStringOptions, shared: false);

    public static NpgsqlTestStore CreateInitialized(string name)
        => new NpgsqlTestStore(name, shared: false)
            .InitializeNpgsql(null, (Func<DbContext>)null, null);

    private NpgsqlTestStore(
        string name,
        string scriptPath = null,
        string additionalSql = null,
        string connectionStringOptions = null,
        bool shared = true)
        : base(name, shared)
    {
        Name = name;

        if (scriptPath is not null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            _scriptPath = Path.Combine(Path.GetDirectoryName(typeof(NpgsqlTestStore).GetTypeInfo().Assembly.Location), scriptPath);
        }

        _additionalSql = additionalSql;

        // ReSharper disable VirtualMemberCallInConstructor
        ConnectionString = CreateConnectionString(Name, connectionStringOptions);
        Connection = new NpgsqlConnection(ConnectionString);
        // ReSharper restore VirtualMemberCallInConstructor
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public NpgsqlTestStore InitializeNpgsql(
        IServiceProvider serviceProvider,
        Func<DbContext> createContext,
        Action<DbContext> seed)
        => (NpgsqlTestStore)Initialize(serviceProvider, createContext, seed);

    // ReSharper disable once UnusedMember.Global
    public NpgsqlTestStore InitializeNpgsql(
        IServiceProvider serviceProvider,
        Func<NpgsqlTestStore, DbContext> createContext,
        Action<DbContext> seed)
        => InitializeNpgsql(serviceProvider, () => createContext(this), seed);

    protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
    {
        if (CreateDatabase(clean))
        {
            if (_scriptPath is not null)
            {
                ExecuteScript(_scriptPath);

                if (_additionalSql is not null)
                {
                    Execute(Connection, command => command.ExecuteNonQuery(), _additionalSql);
                }
            }
            else
            {
                using var context = createContext();
                context.Database.EnsureCreatedResiliently();

                if (_additionalSql is not null)
                {
                    Execute(Connection, command => command.ExecuteNonQuery(), _additionalSql);
                }

                seed?.Invoke(context);
            }
        }
    }

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => builder.UseNpgsql(
            Connection, b => b.ApplyConfiguration()
                .CommandTimeout(CommandTimeout)
                // The tests are written with the assumption that NULLs are sorted first (SQL Server and .NET behavior), but PostgreSQL
                // sorts NULLs last by default. This configures the provider to emit NULLS FIRST.
                .ReverseNullOrdering())
            .UseCockroach();

    private static string GetScratchDbName()
    {
        string name;
        do
        {
            name = "Scratch_" + Guid.NewGuid();
        }
        while (DatabaseExists(name));

        return name;
    }

    private bool CreateDatabase(Action<DbContext> clean)
    {
        using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
        {
            if (DatabaseExists(Name))
            {
                if (_scriptPath is not null)
                {
                    return false;
                }

                using (var context = new DbContext(
                           AddProviderOptions(
                                   new DbContextOptionsBuilder()
                                       .EnableServiceProviderCaching(false))
                               .Options))
                {
                    clean?.Invoke(context);
                    Clean(context);
                    return true;
                }
            }

            ExecuteNonQuery(master, GetCreateDatabaseStatement(Name));
            WaitForExists((NpgsqlConnection)Connection);
        }

        return true;
    }

    private static void WaitForExists(NpgsqlConnection connection)
        => WaitForExistsImplementation(connection);

    private static void WaitForExistsImplementation(NpgsqlConnection connection)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }

                NpgsqlConnection.ClearPool(connection);

                connection.Open();
                connection.Close();
                return;
            }
            catch (PostgresException e)
            {
                if (++retryCount >= 30
                    || e.SqlState != "08001" && e.SqlState != "08000" && e.SqlState != "08006")
                {
                    throw;
                }

                Thread.Sleep(100);
            }
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public void ExecuteScript(string scriptPath)
    {
        var script = File.ReadAllText(scriptPath);
        Execute(
            Connection, command =>
            {
                foreach (var batch in
                         new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromMilliseconds(1000.0))
                             .Split(script).Where(b => !string.IsNullOrEmpty(b)))
                {
                    command.CommandText = batch;
                    command.ExecuteNonQuery();
                }

                return 0;
            }, "");
    }

    // ReSharper disable once UnusedMember.Local
    private static void Clean(string name)
    {
        var options = new DbContextOptionsBuilder()
            .UseNpgsql(CreateConnectionString(name), b => b.ApplyConfiguration())
            .UseInternalServiceProvider(
                new ServiceCollection()
                    .AddEntityFrameworkNpgsql()
                    .AddEntityFrameworkCockroach()
                    .BuildServiceProvider())
            .Options;

        using (var context = new DbContext(options))
        {
            context.Database.EnsureClean();
        }
    }

    private static string GetCreateDatabaseStatement(string name)
        => $@"CREATE DATABASE ""{name}""";

    private static bool DatabaseExists(string name)
    {
        using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
        {
            return ExecuteScalar<long>(master, $@"SELECT COUNT(*) FROM pg_database WHERE datname = '{name}'") > 0;
        }
    }

    public void DeleteDatabase()
    {
        if (!DatabaseExists(Name))
        {
            return;
        }

        using (var master = new NpgsqlConnection(CreateAdminConnectionString()))
        {
            ExecuteNonQuery(master, GetDisconnectDatabaseSql(Name));
            ExecuteNonQuery(master, GetDropDatabaseSql(Name));

            NpgsqlConnection.ClearAllPools();
        }
    }

    // Kill all connection to the database
    // TODO: Pre-9.2 PG has column name procid instead of pid
    private static string GetDisconnectDatabaseSql(string name)
        => $@"
REVOKE CONNECT ON DATABASE ""{name}"" FROM PUBLIC;
SELECT pg_terminate_backend (pg_stat_activity.pid)
   FROM pg_stat_activity
   WHERE datname = '{name}'";

    private static string GetDropDatabaseSql(string name)
        => $@"DROP DATABASE ""{name}""";

    public override void OpenConnection()
        => Connection.Open();

    public override Task OpenConnectionAsync()
        => Connection.OpenAsync();

    // ReSharper disable once UnusedMember.Global
    public T ExecuteScalar<T>(string sql, params object[] parameters)
        => ExecuteScalar<T>(Connection, sql, parameters);

    private static T ExecuteScalar<T>(DbConnection connection, string sql, params object[] parameters)
        => Execute(connection, command => (T)command.ExecuteScalar(), sql, false, parameters);

    // ReSharper disable once UnusedMember.Global
    public Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
        => ExecuteScalarAsync<T>(Connection, sql, parameters);

    private static Task<T> ExecuteScalarAsync<T>(DbConnection connection, string sql, object[] parameters = null)
        => ExecuteAsync(connection, async command => (T)await command.ExecuteScalarAsync(), sql, false, parameters);

    // ReSharper disable once UnusedMethodReturnValue.Global
    public int ExecuteNonQuery(string sql, params object[] parameters)
        => ExecuteNonQuery(Connection, sql, parameters);

    private static int ExecuteNonQuery(DbConnection connection, string sql, object[] parameters = null)
        => Execute(connection, command => command.ExecuteNonQuery(), sql, false, parameters);

    // ReSharper disable once UnusedMember.Global
    public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        => ExecuteNonQueryAsync(Connection, sql, parameters);

    private static Task<int> ExecuteNonQueryAsync(DbConnection connection, string sql, object[] parameters = null)
        => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, false, parameters);

    // ReSharper disable once UnusedMember.Global
    public IEnumerable<T> Query<T>(string sql, params object[] parameters)
        => Query<T>(Connection, sql, parameters);

    private static IEnumerable<T> Query<T>(DbConnection connection, string sql, object[] parameters = null)
        => Execute(
            connection, command =>
            {
                using (var dataReader = command.ExecuteReader())
                {
                    var results = Enumerable.Empty<T>();
                    while (dataReader.Read())
                    {
                        results = results.Concat(new[] { dataReader.GetFieldValue<T>(0) });
                    }

                    return results;
                }
            }, sql, false, parameters);

    // ReSharper disable once UnusedMember.Global
    public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
        => QueryAsync<T>(Connection, sql, parameters);

    private static Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, object[] parameters = null)
        => ExecuteAsync(
            connection, async command =>
            {
                await using (var dataReader = await command.ExecuteReaderAsync())
                {
                    var results = Enumerable.Empty<T>();
                    while (await dataReader.ReadAsync())
                    {
                        results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                    }

                    return results;
                }
            }, sql, false, parameters);

    private static T Execute<T>(
        DbConnection connection,
        Func<DbCommand, T> execute,
        string sql,
        bool useTransaction = false,
        object[] parameters = null)
        => ExecuteCommand(connection, execute, sql, useTransaction, parameters);

    private static T ExecuteCommand<T>(
        DbConnection connection,
        Func<DbCommand, T> execute,
        string sql,
        bool useTransaction,
        object[] parameters)
    {
        if (connection.State != ConnectionState.Closed)
        {
            connection.Close();
        }

        connection.Open();
        try
        {
            using (var transaction = useTransaction ? connection.BeginTransaction() : null)
            {
                T result;
                using (var command = CreateCommand(connection, sql, parameters))
                {
                    command.Transaction = transaction;
                    result = execute(command);
                }

                transaction?.Commit();

                return result;
            }
        }
        finally
        {
            if (connection.State == ConnectionState.Closed
                && connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }

    private static Task<T> ExecuteAsync<T>(
        DbConnection connection,
        Func<DbCommand, Task<T>> executeAsync,
        string sql,
        bool useTransaction = false,
        IReadOnlyList<object> parameters = null)
        => ExecuteCommandAsync(connection, executeAsync, sql, useTransaction, parameters);

    private static async Task<T> ExecuteCommandAsync<T>(
        DbConnection connection,
        Func<DbCommand, Task<T>> executeAsync,
        string sql,
        bool useTransaction,
        IReadOnlyList<object> parameters)
    {
        if (connection.State != ConnectionState.Closed)
        {
            connection.Close();
        }

        await connection.OpenAsync();
        try
        {
            await using (var transaction = useTransaction ? connection.BeginTransaction() : null)
            {
                T result;
                await using (var command = CreateCommand(connection, sql, parameters))
                {
                    result = await executeAsync(command);
                }

                transaction?.Commit();

                return result;
            }
        }
        finally
        {
            if (connection.State == ConnectionState.Closed
                && connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }

    private static DbCommand CreateCommand(
        DbConnection connection,
        string commandText,
        IReadOnlyList<object> parameters = null)
    {
        var command = (NpgsqlCommand)connection.CreateCommand();

        command.CommandText = commandText;
        command.CommandTimeout = CommandTimeout;

        if (parameters is not null)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                command.Parameters.AddWithValue("p" + i, parameters[i]);
            }
        }

        return command;
    }

    public static string CreateConnectionString(string name, string options = null)
    {
        var builder = new NpgsqlConnectionStringBuilder(TestEnvironment.DefaultConnection) { Database = name };

        if (options is not null)
        {
            builder.Options = options;
        }

        return builder.ConnectionString;
    }

    private static string CreateAdminConnectionString()
        => CreateConnectionString("postgres");

    public override void Clean(DbContext context)
        => context.Database.EnsureClean();
}
