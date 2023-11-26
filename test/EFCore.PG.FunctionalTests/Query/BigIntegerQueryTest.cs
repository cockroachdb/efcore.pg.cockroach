using System.Numerics;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class BigIntegerQueryTest : QueryTestBase<BigIntegerQueryTest.BigIntegerQueryFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public BigIntegerQueryTest(BigIntegerQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Abs(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => BigInteger.Abs(e.BigInteger) == 1));

        AssertSql(
            """
SELECT e."Id", e."BigInteger"
FROM "Entities" AS e
WHERE abs(e."BigInteger") = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Pow(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => BigInteger.Pow(e.BigInteger, 2) == 4));

        AssertSql(
            """
SELECT e."Id", e."BigInteger"
FROM "Entities" AS e
WHERE power(e."BigInteger", 2) = 4
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Max(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => BigInteger.Max(e.BigInteger, 1) == 1));

        AssertSql(
            """
SELECT e."Id", e."BigInteger"
FROM "Entities" AS e
WHERE GREATEST(e."BigInteger", 1) = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Min(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => BigInteger.Min(e.BigInteger, 1) == 1));

        AssertSql(
            """
SELECT e."Id", e."BigInteger"
FROM "Entities" AS e
WHERE LEAST(e."BigInteger", 1) = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task IsZero(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.BigInteger.IsZero));

        AssertSql(
            """
SELECT e."Id", e."BigInteger"
FROM "Entities" AS e
WHERE e."BigInteger" = 0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task IsOne(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.BigInteger.IsOne));

        AssertSql(
            """
SELECT e."Id", e."BigInteger"
FROM "Entities" AS e
WHERE e."BigInteger" = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task IsEven(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.BigInteger.IsEven));

        AssertSql(
            """
SELECT e."Id", e."BigInteger"
FROM "Entities" AS e
WHERE e."BigInteger" % 2 = 0
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class BigIntegerQueryContext : PoolableDbContext
    {
        public DbSet<Entity> Entities { get; set; }

        public BigIntegerQueryContext(DbContextOptions options)
            : base(options)
        {
        }

        public static void Seed(BigIntegerQueryContext context)
        {
            context.Entities.AddRange(BigIntegerData.CreateEntities());
            context.SaveChanges();
        }
    }

    public class Entity
    {
        public int Id { get; set; }
        public BigInteger BigInteger { get; set; }
    }

    public class BigIntegerQueryFixture : SharedStoreFixtureBase<BigIntegerQueryContext>, IQueryFixtureBase
    {
        private BigIntegerData _expectedData;

        protected override string StoreName
            => "BigIntegerQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void Seed(BigIntegerQueryContext context)
            => BigIntegerQueryContext.Seed(context);

        public Func<DbContext> GetContextCreator()
            => CreateContext;

        public ISetSource GetExpectedData()
            => _expectedData ??= new BigIntegerData();

        public IReadOnlyDictionary<Type, object> EntitySorters
            => new Dictionary<Type, Func<object, object>> { { typeof(Entity), e => ((Entity)e)?.Id } }
                .ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters
            => new Dictionary<Type, Action<object, object>>
            {
                {
                    typeof(Entity), (e, a) =>
                    {
                        Assert.Equal(e is null, a is null);
                        if (a is not null)
                        {
                            var ee = (Entity)e;
                            var aa = (Entity)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.BigInteger, aa.BigInteger);
                        }
                    }
                }
            }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    protected class BigIntegerData : ISetSource
    {
        public IReadOnlyList<Entity> Entities { get; }

        public BigIntegerData()
        {
            Entities = CreateEntities();
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(Entity))
            {
                return (IQueryable<TEntity>)Entities.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public static IReadOnlyList<Entity> CreateEntities()
            => new List<Entity>
            {
                new() { Id = 1, BigInteger = new BigInteger(0) },
                new() { Id = 2, BigInteger = new BigInteger(1) },
                new() { Id = 3, BigInteger = new BigInteger(2) },
                new() { Id = 4, BigInteger = new BigInteger(3) },
                new() { Id = 5, BigInteger = new BigInteger(-1) },
            };
    }
}
