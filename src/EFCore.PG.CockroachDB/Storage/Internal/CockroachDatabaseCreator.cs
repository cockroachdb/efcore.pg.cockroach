using System.Net.Sockets;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Storage.Internal;

/// <summary>
/// 
/// </summary>
public class CockroachDatabaseCreator : NpgsqlDatabaseCreator
{
    private readonly INpgsqlRelationalConnection _connection;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="connection"></param>
    /// <param name="rawSqlCommandBuilder"></param>
    public CockroachDatabaseCreator(RelationalDatabaseCreatorDependencies dependencies, INpgsqlRelationalConnection connection, IRawSqlCommandBuilder rawSqlCommandBuilder) : base(dependencies, connection, rawSqlCommandBuilder)
    {
        _connection = connection;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        => Exists(async: true, cancellationToken);

    private async Task<bool> Exists(bool async, CancellationToken cancellationToken = default)
    {
        // When checking whether a database exists, pooling must be off, otherwise we may
        // attempt to reuse a pooled connection, which may be broken (this happened in the tests).
        // If Pooling is off, but Multiplexing is on - NpgsqlConnectionStringBuilder.Validate will throw,
        // so we turn off Multiplexing as well.
        var unpooledCsb = new NpgsqlConnectionStringBuilder(_connection.ConnectionString)
        {
            Pooling = false,
            Multiplexing = false
        };

        using var _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
        var unpooledRelationalConnection = _connection.CloneWith(unpooledCsb.ToString());
        try
        {
            if (async)
            {
                await unpooledRelationalConnection.OpenAsync(errorsExpected: true, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                unpooledRelationalConnection.Open(errorsExpected: true);
            }

            // Workaround for CockroachDB issue that doesn't return error when database name is invalid
            // https://github.com/cockroachdb/cockroach/issues/109992
            if (async)
            {
                await unpooledRelationalConnection.DbConnection.ReloadTypesAsync()
                    .ConfigureAwait(false);;
            }
            else
            {
                unpooledRelationalConnection.DbConnection.ReloadTypes();
            }

            return true;
        }
        catch (PostgresException e)
        {
            if (IsDoesNotExist(e))
            {
                return false;
            }

            throw;
        }
        catch (NpgsqlException e) when (
            // This can happen when Npgsql attempts to connect to multiple hosts
            e.InnerException is AggregateException ae &&
            ae.InnerExceptions.Any(ie => ie is PostgresException pe && IsDoesNotExist(pe)))
        {
            return false;
        }
        catch (NpgsqlException e) when (
            e.InnerException is IOException { InnerException: SocketException { SocketErrorCode: SocketError.ConnectionReset } })
        {
            // Pretty awful hack around #104
            return false;
        }
        finally
        {
            if (async)
            {
                await unpooledRelationalConnection.CloseAsync().ConfigureAwait(false);
                await unpooledRelationalConnection.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                unpooledRelationalConnection.Close();
                unpooledRelationalConnection.Dispose();
            }
        }
    }
    
    // Login failed is thrown when database does not exist (See Issue #776)
    private static bool IsDoesNotExist(PostgresException exception) => exception.SqlState == "3D000";
}