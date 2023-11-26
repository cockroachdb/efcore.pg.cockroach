using System.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

// ReSharper disable MethodSupportsCancellation
// ReSharper disable AccessToDisposedClosure
namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class ExecutionStrategyTest : IClassFixture<ExecutionStrategyTest.ExecutionStrategyFixture>
{
    public ExecutionStrategyTest(ExecutionStrategyFixture fixture)
    {
        Fixture = fixture;
        Fixture.TestStore.CloseConnection();
        Fixture.TestSqlLoggerFactory.Clear();
    }

    protected ExecutionStrategyFixture Fixture { get; }

    [ConditionalTheory]
    [MemberData(nameof(DataGenerator.GetBoolCombinations), 1, MemberType = typeof(DataGenerator))]
    public void Handles_commit_failure(bool realFailure)
    {
        // Use all overloads of ExecuteInTransaction
        Test_commit_failure(
            realFailure, (e, db) => e.ExecuteInTransaction(
                () => { db.SaveChanges(false); },
                () => db.Products.AsNoTracking().Any()));

        Test_commit_failure(
            realFailure, (e, db) => e.ExecuteInTransaction(
                () => db.SaveChanges(false),
                () => db.Products.AsNoTracking().Any()));

        Test_commit_failure(
            realFailure, (e, db) => e.ExecuteInTransaction(
                db,
                c => { c.SaveChanges(false); },
                c => c.Products.AsNoTracking().Any()));

        Test_commit_failure(
            realFailure, (e, db) => e.ExecuteInTransaction(
                db,
                c => c.SaveChanges(false),
                c => c.Products.AsNoTracking().Any()));

        Test_commit_failure(
            realFailure, (e, db) => e.ExecuteInTransaction(
                () => { db.SaveChanges(false); },
                () => db.Products.AsNoTracking().Any(),
                IsolationLevel.Serializable));

        Test_commit_failure(
            realFailure, (e, db) => e.ExecuteInTransaction(
                () => db.SaveChanges(false),
                () => db.Products.AsNoTracking().Any(),
                IsolationLevel.Serializable));

        Test_commit_failure(
            realFailure, (e, db) => e.ExecuteInTransaction(
                db,
                c => { c.SaveChanges(false); },
                c => c.Products.AsNoTracking().Any(),
                IsolationLevel.Serializable));

        Test_commit_failure(
            realFailure, (e, db) => e.ExecuteInTransaction(
                db,
                c => c.SaveChanges(false),
                c => c.Products.AsNoTracking().Any(),
                IsolationLevel.Serializable));
    }

    private void Test_commit_failure(bool realFailure, Action<TestNpgsqlRetryingExecutionStrategy, ExecutionStrategyContext> execute)
    {
        CleanContext();

        using (var context = CreateContext())
        {
            var connection = (TestNpgsqlConnection)context.GetService<INpgsqlRelationalConnection>();

            connection.CommitFailures.Enqueue(new bool?[] { realFailure });
            Fixture.TestSqlLoggerFactory.Clear();

            context.Products.Add(new Product());
            execute(new TestNpgsqlRetryingExecutionStrategy(context), context);
            context.ChangeTracker.AcceptAllChanges();

            var retryMessage =
                "A transient exception occurred during execution. The operation will be retried after 0ms."
                + Environment.NewLine
                + "Npgsql.PostgresException (0x80004005): XX000";
            if (realFailure)
            {
                var logEntry = Fixture.TestSqlLoggerFactory.Log.Single(l => l.Id == CoreEventId.ExecutionStrategyRetrying);
                Assert.Contains(retryMessage, logEntry.Message);
                Assert.Equal(LogLevel.Information, logEntry.Level);
            }
            else
            {
                Assert.Empty(Fixture.TestSqlLoggerFactory.Log.Where(l => l.Id == CoreEventId.ExecutionStrategyRetrying));
            }

            Assert.Equal(realFailure ? 3 : 2, connection.OpenCount);
        }

        using (var context = CreateContext())
        {
            Assert.Equal(1, context.Products.Count());
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(DataGenerator.GetBoolCombinations), 1, MemberType = typeof(DataGenerator))]
    public async Task Handles_commit_failure_async(bool realFailure)
    {
        // Use all overloads of ExecuteInTransactionAsync
        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                () => db.SaveChangesAsync(false),
                () => db.Products.AsNoTracking().AnyAsync()));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                async _ => { await db.SaveChangesAsync(false); },
                _ => db.Products.AsNoTracking().AnyAsync(),
                CancellationToken.None));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                ct => db.SaveChangesAsync(false, ct),
                _ => db.Products.AsNoTracking().AnyAsync(),
                CancellationToken.None));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                db,
                async (c, ct) => { await c.SaveChangesAsync(false, ct); },
                (c, _) => c.Products.AsNoTracking().AnyAsync(),
                CancellationToken.None));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                db,
                (c, ct) => c.SaveChangesAsync(false, ct),
                (c, _) => c.Products.AsNoTracking().AnyAsync(),
                CancellationToken.None));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                () => db.SaveChangesAsync(false),
                () => db.Products.AsNoTracking().AnyAsync(),
                IsolationLevel.Serializable));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                async ct => { await db.SaveChangesAsync(false, ct); },
                ct => db.Products.AsNoTracking().AnyAsync(ct),
                IsolationLevel.Serializable,
                CancellationToken.None));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                ct => db.SaveChangesAsync(false, ct),
                ct => db.Products.AsNoTracking().AnyAsync(ct),
                IsolationLevel.Serializable,
                CancellationToken.None));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                db,
                async (c, ct) => { await c.SaveChangesAsync(false, ct); },
                (c, ct) => c.Products.AsNoTracking().AnyAsync(ct),
                IsolationLevel.Serializable,
                CancellationToken.None));

        await Test_commit_failure_async(
            realFailure, (e, db) => e.ExecuteInTransactionAsync(
                db,
                (c, ct) => c.SaveChangesAsync(false, ct),
                (c, ct) => c.Products.AsNoTracking().AnyAsync(ct),
                IsolationLevel.Serializable,
                CancellationToken.None));
    }

    private async Task Test_commit_failure_async(
        bool realFailure,
        Func<TestNpgsqlRetryingExecutionStrategy, ExecutionStrategyContext, Task> execute)
    {
        CleanContext();

        await using (var context = CreateContext())
        {
            var connection = (TestNpgsqlConnection)context.GetService<INpgsqlRelationalConnection>();

            connection.CommitFailures.Enqueue(new bool?[] { realFailure });
            Fixture.TestSqlLoggerFactory.Clear();

            context.Products.Add(new Product());
            await execute(new TestNpgsqlRetryingExecutionStrategy(context), context);
            context.ChangeTracker.AcceptAllChanges();

            var retryMessage =
                "A transient exception occurred during execution. The operation will be retried after 0ms."
                + Environment.NewLine
                + "Npgsql.PostgresException (0x80004005): XX000";
            if (realFailure)
            {
                var logEntry = Fixture.TestSqlLoggerFactory.Log.Single(l => l.Id == CoreEventId.ExecutionStrategyRetrying);
                Assert.Contains(retryMessage, logEntry.Message);
                Assert.Equal(LogLevel.Information, logEntry.Level);
            }
            else
            {
                Assert.Empty(Fixture.TestSqlLoggerFactory.Log.Where(l => l.Id == CoreEventId.ExecutionStrategyRetrying));
            }

            Assert.Equal(realFailure ? 3 : 2, connection.OpenCount);
        }

        await using (var context = CreateContext())
        {
            Assert.Equal(1, await context.Products.CountAsync());
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(DataGenerator.GetBoolCombinations), 1, MemberType = typeof(DataGenerator))]
    public void Handles_commit_failure_multiple_SaveChanges(bool realFailure)
    {
        CleanContext();

        using var context1 = CreateContext();
        var connection = (TestNpgsqlConnection)context1.GetService<INpgsqlRelationalConnection>();

        using (var context2 = CreateContext())
        {
            connection.CommitFailures.Enqueue(new bool?[] { realFailure });

            context1.Products.Add(new Product());
            context2.Products.Add(new Product());

            new TestNpgsqlRetryingExecutionStrategy(context1).ExecuteInTransaction(
                context1,
                c1 =>
                {
                    context2.Database.UseTransaction(null);
                    context2.Database.UseTransaction(context1.Database.CurrentTransaction.GetDbTransaction());

                    c1.SaveChanges(false);

                    return context2.SaveChanges(false);
                },
                c => c.Products.AsNoTracking().Any());

            context1.ChangeTracker.AcceptAllChanges();
            context2.ChangeTracker.AcceptAllChanges();
        }

        using var context = CreateContext();
        Assert.Equal(2, context.Products.Count());
    }

    [ConditionalTheory]
    [MemberData(nameof(DataGenerator.GetBoolCombinations), 4, MemberType = typeof(DataGenerator))]
    public async Task Retries_SaveChanges_on_execution_failure(
        bool realFailure,
        bool externalStrategy,
        bool openConnection,
        bool async)
    {
        CleanContext();

        await using (var context = CreateContext())
        {
            var connection = (TestNpgsqlConnection)context.GetService<INpgsqlRelationalConnection>();

            connection.ExecutionFailures.Enqueue(new bool?[] { null, realFailure });

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            if (openConnection)
            {
                if (async)
                {
                    await context.Database.OpenConnectionAsync();
                }
                else
                {
                    context.Database.OpenConnection();
                }

                Assert.Equal(ConnectionState.Open, context.Database.GetDbConnection().State);
            }

            context.Products.Add(new Product());
            context.Products.Add(new Product());

            if (async)
            {
                if (externalStrategy)
                {
                    await new TestNpgsqlRetryingExecutionStrategy(context).ExecuteInTransactionAsync(
                        context,
                        (c, ct) => c.SaveChangesAsync(false, ct),
                        (_, _) =>
                        {
                            Assert.True(false);
                            return Task.FromResult(false);
                        });

                    context.ChangeTracker.AcceptAllChanges();
                }
                else
                {
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                if (externalStrategy)
                {
                    new TestNpgsqlRetryingExecutionStrategy(context).ExecuteInTransaction(
                        context,
                        c => c.SaveChanges(false),
                        _ =>
                        {
                            Assert.True(false);
                            return false;
                        });

                    context.ChangeTracker.AcceptAllChanges();
                }
                else
                {
                    context.SaveChanges();
                }
            }

            Assert.Equal(2, connection.OpenCount);
            Assert.Equal(4, connection.ExecutionCount);

            Assert.Equal(
                openConnection
                    ? ConnectionState.Open
                    : ConnectionState.Closed, context.Database.GetDbConnection().State);

            if (openConnection)
            {
                if (async)
                {
                    context.Database.CloseConnection();
                }
                else
                {
                    await context.Database.CloseConnectionAsync();
                }
            }

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        await using (var context = CreateContext())
        {
            Assert.Equal(2, context.Products.Count());
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(DataGenerator.GetBoolCombinations), 2, MemberType = typeof(DataGenerator))]
    public async Task Retries_query_on_execution_failure(bool externalStrategy, bool async)
    {
        CleanContext();

        await using (var context = CreateContext())
        {
            context.Products.Add(new Product());
            context.Products.Add(new Product());

            context.SaveChanges();
        }

        await using (var context = CreateContext())
        {
            var connection = (TestNpgsqlConnection)context.GetService<INpgsqlRelationalConnection>();

            connection.ExecutionFailures.Enqueue(new bool?[] { true });

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            List<Product> list;
            if (async)
            {
                if (externalStrategy)
                {
                    list = await new TestNpgsqlRetryingExecutionStrategy(context)
                        .ExecuteAsync(context, (c, ct) => c.Products.ToListAsync(ct), null);
                }
                else
                {
                    list = await context.Products.ToListAsync();
                }
            }
            else
            {
                if (externalStrategy)
                {
                    list = new TestNpgsqlRetryingExecutionStrategy(context)
                        .Execute(context, c => c.Products.ToList(), null);
                }
                else
                {
                    list = context.Products.ToList();
                }
            }

            Assert.Equal(2, list.Count);
            Assert.Equal(1, connection.OpenCount);
            Assert.Equal(2, connection.ExecutionCount);

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(DataGenerator.GetBoolCombinations), 2, MemberType = typeof(DataGenerator))]
    public async Task Retries_FromSqlRaw_on_execution_failure(bool externalStrategy, bool async)
    {
        CleanContext();

        await using (var context = CreateContext())
        {
            context.Products.Add(new Product());
            context.Products.Add(new Product());

            context.SaveChanges();
        }

        await using (var context = CreateContext())
        {
            var connection = (TestNpgsqlConnection)context.GetService<INpgsqlRelationalConnection>();

            connection.ExecutionFailures.Enqueue(new bool?[] { true });

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            List<Product> list;
            if (async)
            {
                if (externalStrategy)
                {
                    list = await new TestNpgsqlRetryingExecutionStrategy(context)
                        .ExecuteAsync(
                            context, (c, ct) => c.Set<Product>().FromSqlRaw(
                                @"SELECT ""Id"", ""Name""
                              FROM ""Products""").ToListAsync(ct), null);
                }
                else
                {
                    list = await context.Set<Product>().FromSqlRaw(
                        @"SELECT ""Id"", ""Name""
                              FROM ""Products""").ToListAsync();
                }
            }
            else
            {
                if (externalStrategy)
                {
                    list = new TestNpgsqlRetryingExecutionStrategy(context)
                        .Execute(
                            context, c => c.Set<Product>().FromSqlRaw(
                                @"SELECT ""Id"", ""Name""
                              FROM ""Products""").ToList(), null);
                }
                else
                {
                    list = context.Set<Product>().FromSqlRaw(
                        @"SELECT ""Id"", ""Name""
                              FROM ""Products""").ToList();
                }
            }

            Assert.Equal(2, list.Count);
            Assert.Equal(1, connection.OpenCount);
            Assert.Equal(2, connection.ExecutionCount);

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(DataGenerator.GetBoolCombinations), 2, MemberType = typeof(DataGenerator))]
    public async Task Retries_OpenConnection_on_execution_failure(bool externalStrategy, bool async)
    {
        await using var context = CreateContext();
        var connection = (TestNpgsqlConnection)context.GetService<INpgsqlRelationalConnection>();

        connection.OpenFailures.Enqueue(new bool?[] { true });

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

        if (async)
        {
            if (externalStrategy)
            {
                await new TestNpgsqlRetryingExecutionStrategy(context).ExecuteAsync(
                    context,
                    c => c.Database.OpenConnectionAsync());
            }
            else
            {
                await context.Database.OpenConnectionAsync();
            }
        }
        else
        {
            if (externalStrategy)
            {
                new TestNpgsqlRetryingExecutionStrategy(context).Execute(
                    context,
                    c => c.Database.OpenConnection());
            }
            else
            {
                context.Database.OpenConnection();
            }
        }

        Assert.Equal(2, connection.OpenCount);

        if (async)
        {
            context.Database.CloseConnection();
        }
        else
        {
            await context.Database.CloseConnectionAsync();
        }

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Retries_BeginTransaction_on_execution_failure(bool async)
    {
        await using var context = CreateContext();
        var connection = (TestNpgsqlConnection)context.GetService<INpgsqlRelationalConnection>();

        connection.OpenFailures.Enqueue(new bool?[] { true });

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

        if (async)
        {
            var transaction = await new TestNpgsqlRetryingExecutionStrategy(context).ExecuteAsync(
                context,
                _ => context.Database.BeginTransactionAsync());

            transaction.Dispose();
        }
        else
        {
            var transaction = new TestNpgsqlRetryingExecutionStrategy(context).Execute(
                context,
                _ => context.Database.BeginTransaction());

            transaction.Dispose();
        }

        Assert.Equal(2, connection.OpenCount);

        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [ConditionalFact]
    public void Verification_is_retried_using_same_retry_limit()
    {
        CleanContext();

        using (var context = CreateContext())
        {
            var connection = (TestNpgsqlConnection)context.GetService<INpgsqlRelationalConnection>();

            connection.ExecutionFailures.Enqueue(new bool?[] { true, null, true, true });
            connection.CommitFailures.Enqueue(new bool?[] { true, true, true, true });

            context.Products.Add(new Product());
            Assert.Throws<RetryLimitExceededException>(
                () =>
                    new TestNpgsqlRetryingExecutionStrategy(context, TimeSpan.FromMilliseconds(100))
                        .ExecuteInTransaction(
                            context,
                            c => c.SaveChanges(false),
                            _ => false));
            context.ChangeTracker.AcceptAllChanges();

            Assert.Equal(7, connection.OpenCount);
            Assert.Equal(7, connection.ExecutionCount);
        }

        using (var context = CreateContext())
        {
            Assert.Equal(0, context.Products.Count());
        }
    }

    protected class ExecutionStrategyContext : DbContext
    {
        public ExecutionStrategyContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }

    protected class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    protected virtual ExecutionStrategyContext CreateContext()
        => (ExecutionStrategyContext)Fixture.CreateContext();

    private void CleanContext()
    {
        using var context = CreateContext();
        foreach (var product in context.Products.ToList())
        {
            context.Remove(product);
            context.SaveChanges();
        }
    }

    public class ExecutionStrategyFixture : SharedStoreFixtureBase<DbContext>
    {
        protected override bool UsePooling
            => false;

        protected override string StoreName { get; } = nameof(ExecutionStrategyTest);

        public new RelationalTestStore TestStore
            => (RelationalTestStore)base.TestStore;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override Type ContextType { get; } = typeof(ExecutionStrategyContext);

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<IRelationalTransactionFactory, TestRelationalTransactionFactory>()
                .AddScoped<INpgsqlRelationalConnection, TestNpgsqlConnection>()
                .AddSingleton<IRelationalCommandBuilderFactory, TestRelationalCommandBuilderFactory>();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var options = base.AddOptions(builder);

            new NpgsqlDbContextOptionsBuilder(options)
                .MaxBatchSize(1)
                .ExecutionStrategy(d => new TestNpgsqlRetryingExecutionStrategy(d));

            return options;
        }

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Infrastructure.Name;
    }
}
