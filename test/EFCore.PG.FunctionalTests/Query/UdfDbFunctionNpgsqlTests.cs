﻿using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class UdfDbFunctionNpgsqlTests : UdfDbFunctionTestBase<UdfDbFunctionNpgsqlTests.UdfNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public UdfDbFunctionNpgsqlTests(UdfNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Static

    [SkipForCockroachDb("https://github.com/cockroachdb/cockroach/issues/108416")]
    [ConditionalFact]
    public override void Scalar_Function_Extension_Method_Static()
    {
        base.Scalar_Function_Extension_Method_Static();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE "IsDate"(c."FirstName") = FALSE
""");
    }

    [Fact]
    public override void Scalar_Function_With_Translator_Translates_Static()
    {
        base.Scalar_Function_With_Translator_Translates_Static();

        AssertSql(
            """
@__customerId_0='3'

SELECT length(c."LastName")
FROM "Customers" AS c
WHERE c."Id" = @__customerId_0
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static()
        => base.Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static();

    [Fact]
    public override void Scalar_Function_Constant_Parameter_Static()
    {
        base.Scalar_Function_Constant_Parameter_Static();

        AssertSql(
            """
@__customerId_0='1'

SELECT "CustomerOrderCount"(@__customerId_0)
FROM "Customers" AS c
""");
    }

    [Fact]
    public override void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
    {
        base.Scalar_Function_Anonymous_Type_Select_Correlated_Static();

        AssertSql(
            """
SELECT c."LastName", "CustomerOrderCount"(c."Id") AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = 1
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
    {
        base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static();

        AssertSql(
            """
SELECT c."LastName", "CustomerOrderCount"(1) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = 1
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
    {
        base.Scalar_Function_Anonymous_Type_Select_Parameter_Static();

        AssertSql(
            """
@__customerId_0='1'

SELECT c."LastName", "CustomerOrderCount"(@__customerId_0) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = @__customerId_0
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Anonymous_Type_Select_Nested_Static()
    {
        base.Scalar_Function_Anonymous_Type_Select_Nested_Static();

        AssertSql(
            """
@__starCount_1='3'
@__customerId_0='3'

SELECT c."LastName", "StarValue"(@__starCount_1, "CustomerOrderCount"(@__customerId_0)) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = @__customerId_0
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Where_Correlated_Static()
    {
        base.Scalar_Function_Where_Correlated_Static();

        AssertSql(
            """
SELECT lower(c."Id"::text)
FROM "Customers" AS c
WHERE "IsTopCustomer"(c."Id")
""");
    }

    [Fact(Skip = "https://github.com/dotnet/efcore/issues/25980")]
    public override void Scalar_Function_Where_Not_Correlated_Static()
    {
        base.Scalar_Function_Where_Not_Correlated_Static();

        AssertSql(
            """
@__startDate_0='2000-04-01T00:00:00.0000000' (Nullable = true) (DbType = DateTime)

SELECT c."Id"
FROM "Customers" AS c
WHERE "GetCustomerWithMostOrdersAfterDate"(@__startDate_0) = c."Id"
LIMIT 2
""");
    }

    [SkipForCockroachDb("https://github.com/cockroachdb/cockroach/issues/108416")]
    [ConditionalFact]
    public override void Scalar_Function_Where_Parameter_Static()
    {
        base.Scalar_Function_Where_Parameter_Static();

        AssertSql(
            """
@__period_0='0'

SELECT c."Id"
FROM "Customers" AS c
WHERE c."Id" = "GetCustomerWithMostOrdersAfterDate"("GetReportingPeriodStartDate"(@__period_0))
LIMIT 2
""");
    }

    [SkipForCockroachDb("https://github.com/cockroachdb/cockroach/issues/108416")]
    [ConditionalFact]
    public override void Scalar_Function_Where_Nested_Static()
    {
        base.Scalar_Function_Where_Nested_Static();

        AssertSql(
            """
SELECT c."Id"
FROM "Customers" AS c
WHERE c."Id" = "GetCustomerWithMostOrdersAfterDate"("GetReportingPeriodStartDate"(0))
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Let_Correlated_Static()
    {
        base.Scalar_Function_Let_Correlated_Static();

        AssertSql(
            """
SELECT c."LastName", "CustomerOrderCount"(c."Id") AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = 2
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Let_Not_Correlated_Static()
    {
        base.Scalar_Function_Let_Not_Correlated_Static();

        AssertSql(
            """
SELECT c."LastName", "CustomerOrderCount"(2) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = 2
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Let_Not_Parameter_Static()
    {
        base.Scalar_Function_Let_Not_Parameter_Static();

        AssertSql(
            """
@__customerId_0='2'

SELECT c."LastName", "CustomerOrderCount"(@__customerId_0) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = @__customerId_0
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Let_Nested_Static()
    {
        base.Scalar_Function_Let_Nested_Static();

        AssertSql(
            """
@__starCount_0='3'
@__customerId_1='1'

SELECT c."LastName", "StarValue"(@__starCount_0, "CustomerOrderCount"(@__customerId_1)) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = @__customerId_1
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
    {
        base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Static();

        AssertSql(
            """
SELECT c."Id"
FROM "Customers" AS c
ORDER BY c."Id" NULLS FIRST
""");
    }

    [Fact]
    public override void Scalar_Nested_Function_BCL_UDF_Static()
    {
        base.Scalar_Nested_Function_BCL_UDF_Static();

        AssertSql(
            """
SELECT c."Id"
FROM "Customers" AS c
WHERE 3 = abs("CustomerOrderCount"(c."Id"))
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Nested_Function_UDF_BCL_Static()
    {
        base.Scalar_Nested_Function_UDF_BCL_Static();

        AssertSql(
            """
SELECT c."Id"
FROM "Customers" AS c
WHERE 3 = "CustomerOrderCount"(abs(c."Id"))
LIMIT 2
""");
    }

    [Fact]
    public override void Nullable_navigation_property_access_preserves_schema_for_sql_function()
    {
        base.Nullable_navigation_property_access_preserves_schema_for_sql_function();

        AssertSql(
            """
SELECT dbo."IdentityString"(c."FirstName")
FROM "Orders" AS o
INNER JOIN "Customers" AS c ON o."CustomerId" = c."Id"
ORDER BY o."Id" NULLS FIRST
LIMIT 1
""");
    }

    #endregion

    #region Instance

    [Fact]
    public override void Scalar_Function_Non_Static()
    {
        base.Scalar_Function_Non_Static();

        AssertSql(
            """
SELECT "StarValue"(4, c."Id") AS "Id", "DollarValue"(2, c."LastName") AS "LastName"
FROM "Customers" AS c
WHERE c."Id" = 1
LIMIT 2
""");
    }

    [SkipForCockroachDb("https://github.com/cockroachdb/cockroach/issues/108416")]
    [ConditionalFact]
    public override void Scalar_Function_Extension_Method_Instance()
    {
        base.Scalar_Function_Extension_Method_Instance();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE "IsDate"(c."FirstName") = FALSE
""");
    }

    [Fact]
    public override void Scalar_Function_With_Translator_Translates_Instance()
    {
        base.Scalar_Function_With_Translator_Translates_Instance();

        AssertSql(
            """
@__customerId_0='3'

SELECT length(c."LastName")
FROM "Customers" AS c
WHERE c."Id" = @__customerId_0
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Constant_Parameter_Instance()
    {
        base.Scalar_Function_Constant_Parameter_Instance();

        AssertSql(
            """
@__customerId_1='1'

SELECT "CustomerOrderCount"(@__customerId_1)
FROM "Customers" AS c
""");
    }

    [Fact]
    public override void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
    {
        base.Scalar_Function_Anonymous_Type_Select_Correlated_Instance();

        AssertSql(
            """
SELECT c."LastName", "CustomerOrderCount"(c."Id") AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = 1
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
    {
        base.Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance();

        AssertSql(
            """
SELECT c."LastName", "CustomerOrderCount"(1) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = 1
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
    {
        base.Scalar_Function_Anonymous_Type_Select_Parameter_Instance();

        AssertSql(
            """
@__customerId_0='1'

SELECT c."LastName", "CustomerOrderCount"(@__customerId_0) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = @__customerId_0
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Anonymous_Type_Select_Nested_Instance()
    {
        base.Scalar_Function_Anonymous_Type_Select_Nested_Instance();

        AssertSql(
            """
@__starCount_2='3'
@__customerId_0='3'

SELECT c."LastName", "StarValue"(@__starCount_2, "CustomerOrderCount"(@__customerId_0)) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = @__customerId_0
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Where_Correlated_Instance()
    {
        base.Scalar_Function_Where_Correlated_Instance();

        AssertSql(
            """
SELECT lower(c."Id"::text)
FROM "Customers" AS c
WHERE "IsTopCustomer"(c."Id")
""");
    }

    [Fact(Skip = "https://github.com/dotnet/efcore/issues/25980")]
    public override void Scalar_Function_Where_Not_Correlated_Instance()
    {
        base.Scalar_Function_Where_Not_Correlated_Instance();

        AssertSql(
            """
@__startDate_1='2000-04-01T00:00:00.0000000' (Nullable = true) (DbType = DateTime)

SELECT c."Id"
FROM "Customers" AS c
WHERE "GetCustomerWithMostOrdersAfterDate"(@__startDate_1) = c."Id"
LIMIT 2
""");
    }

    [SkipForCockroachDb("https://github.com/cockroachdb/cockroach/issues/108416")]
    [ConditionalFact]
    public override void Scalar_Function_Where_Parameter_Instance()
    {
        base.Scalar_Function_Where_Parameter_Instance();

        AssertSql(
            """
@__period_1='0'

SELECT c."Id"
FROM "Customers" AS c
WHERE c."Id" = "GetCustomerWithMostOrdersAfterDate"("GetReportingPeriodStartDate"(@__period_1))
LIMIT 2
""");
    }

    [SkipForCockroachDb("https://github.com/cockroachdb/cockroach/issues/108416")]
    [ConditionalFact]
    public override void Scalar_Function_Where_Nested_Instance()
    {
        base.Scalar_Function_Where_Nested_Instance();

        AssertSql(
            """
SELECT c."Id"
FROM "Customers" AS c
WHERE c."Id" = "GetCustomerWithMostOrdersAfterDate"("GetReportingPeriodStartDate"(0))
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Let_Correlated_Instance()
    {
        base.Scalar_Function_Let_Correlated_Instance();

        AssertSql(
            """
SELECT c."LastName", "CustomerOrderCount"(c."Id") AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = 2
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Let_Not_Correlated_Instance()
    {
        base.Scalar_Function_Let_Not_Correlated_Instance();

        AssertSql(
            """
SELECT c."LastName", "CustomerOrderCount"(2) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = 2
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Let_Not_Parameter_Instance()
    {
        base.Scalar_Function_Let_Not_Parameter_Instance();

        AssertSql(
            """
@__customerId_1='2'

SELECT c."LastName", "CustomerOrderCount"(@__customerId_1) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = @__customerId_1
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Function_Let_Nested_Instance()
    {
        base.Scalar_Function_Let_Nested_Instance();

        AssertSql(
            """
@__starCount_1='3'
@__customerId_2='1'

SELECT c."LastName", "StarValue"(@__starCount_1, "CustomerOrderCount"(@__customerId_2)) AS "OrderCount"
FROM "Customers" AS c
WHERE c."Id" = @__customerId_2
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
    {
        base.Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance();

        AssertSql(
            """
SELECT c."Id"
FROM "Customers" AS c
ORDER BY c."Id" NULLS FIRST
""");
    }

    [Fact]
    public override void Scalar_Nested_Function_BCL_UDF_Instance()
    {
        base.Scalar_Nested_Function_BCL_UDF_Instance();

        AssertSql(
            """
SELECT c."Id"
FROM "Customers" AS c
WHERE 3 = abs("CustomerOrderCount"(c."Id"))
LIMIT 2
""");
    }

    [Fact]
    public override void Scalar_Nested_Function_UDF_BCL_Instance()
    {
        base.Scalar_Nested_Function_UDF_BCL_Instance();

        AssertSql(
            """
SELECT c."Id"
FROM "Customers" AS c
WHERE 3 = "CustomerOrderCount"(abs(c."Id"))
LIMIT 2
""");
    }

    #endregion

#if RELEASE
    [ConditionalFact(Skip = "https://github.com/dotnet/efcore/pull/30388")]
    public override void Scalar_Function_with_nullable_value_return_type_throws() {}
#endif

    protected class NpgsqlUDFSqlContext : UDFSqlContext
    {
        public NpgsqlUDFSqlContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // IsDate is a built-in SQL Server function, that in the base class is mapped as built-in, which means we
            // don't get any quotes. We remap it as non-built-in by including a (null) schema.
            var isDateMethodInfo = typeof(UDFSqlContext).GetMethod(nameof(IsDateStatic));
            modelBuilder.HasDbFunction(isDateMethodInfo)
                .HasTranslation(
                    args => new SqlFunctionExpression(
                        schema: null,
                        "IsDate",
                        args,
                        nullable: true,
                        argumentsPropagateNullability: args.Select(_ => true).ToList(),
                        isDateMethodInfo.ReturnType,
                        typeMapping: null));

            var isDateMethodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(IsDateInstance));
            modelBuilder.HasDbFunction(isDateMethodInfo2)
                .HasTranslation(
                    args => new SqlFunctionExpression(
                        schema: null,
                        "IsDate",
                        args,
                        nullable: true,
                        argumentsPropagateNullability: args.Select(_ => true).ToList(),
                        isDateMethodInfo2.ReturnType,
                        typeMapping: null));

            // Base class maps to len(), but in PostgreSQL it's called length()
            var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));
            modelBuilder.HasDbFunction(methodInfo)
                .HasTranslation(
                    args => new SqlFunctionExpression(
                        "length",
                        args,
                        nullable: true,
                        argumentsPropagateNullability: args.Select(_ => true).ToList(),
                        methodInfo.ReturnType,
                        typeMapping: null));

            var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));
            modelBuilder.HasDbFunction(methodInfo2)
                .HasTranslation(
                    args => new SqlFunctionExpression(
                        "length",
                        args,
                        nullable: true,
                        argumentsPropagateNullability: args.Select(_ => true).ToList(),
                        methodInfo2.ReturnType,
                        typeMapping: null));
        }
    }
    
    #region Skip for CockroachDB

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void DbSet_mapped_to_function()
    {
        base.DbSet_mapped_to_function();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Correlated_Func_Call_With_Navigation()
    {
        base.QF_Correlated_Func_Call_With_Navigation();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Correlated_Nested_Func_Call()
    {
        base.QF_Correlated_Nested_Func_Call();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Correlated_Select_In_Anonymous()
    {
        base.QF_Correlated_Select_In_Anonymous();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_CrossApply_Correlated_Select_Anonymous()
    {
        base.QF_CrossApply_Correlated_Select_Anonymous();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_CrossApply_Correlated_Select_QF_Type()
    {
        base.QF_CrossApply_Correlated_Select_QF_Type();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_CrossApply_Correlated_Select_Result()
    {
        base.QF_CrossApply_Correlated_Select_Result();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_CrossJoin_Not_Correlated()
    {
        base.QF_CrossJoin_Not_Correlated();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_CrossJoin_Parameter()
    {
        base.QF_CrossJoin_Parameter();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Join()
    {
        base.QF_Join();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_LeftJoin_Select_Anonymous()
    {
        base.QF_LeftJoin_Select_Anonymous();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_LeftJoin_Select_Result()
    {
        base.QF_LeftJoin_Select_Result();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Nested()
    {
        base.QF_Nested();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_OuterApply_Correlated_Select_Anonymous()
    {
        base.QF_OuterApply_Correlated_Select_Anonymous();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_OuterApply_Correlated_Select_Entity()
    {
        base.QF_OuterApply_Correlated_Select_Entity();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_OuterApply_Correlated_Select_QF()
    {
        base.QF_OuterApply_Correlated_Select_QF();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous()
    {
        base.QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Select_Correlated_Subquery_In_Anonymous()
    {
        base.QF_Select_Correlated_Subquery_In_Anonymous();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF()
    {
        base.QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Stand_Alone()
    {
        base.QF_Stand_Alone();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void QF_Stand_Alone_Parameter()
    {
        base.QF_Stand_Alone_Parameter();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void TVF_with_argument_being_a_subquery_with_navigation_in_projection_groupby_aggregate()
    {
        base.TVF_with_argument_being_a_subquery_with_navigation_in_projection_groupby_aggregate();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void TVF_with_navigation_in_projection_groupby_aggregate()
    {
        base.TVF_with_navigation_in_projection_groupby_aggregate();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void Udf_with_argument_being_comparison_of_nullable_columns()
    {
        base.Udf_with_argument_being_comparison_of_nullable_columns();
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    public override void Udf_with_argument_being_comparison_to_null_parameter()
    {
        base.Udf_with_argument_being_comparison_to_null_parameter();
    }

    #endregion

    public class UdfNpgsqlFixture : UdfFixtureBase
    {
        protected override string StoreName { get; } = "UDFDbFunctionNpgsqlTests";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override Type ContextType { get; } = typeof(NpgsqlUDFSqlContext);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
            // supported.
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasColumnType("timestamp without time zone");

            // The following should make us send 'timestamp without time zone' for these functions, but it doesn't:
            // https://github.com/dotnet/efcore/issues/25980
            var typeMappingSource = context.GetService<IRelationalTypeMappingSource>();
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic)))
                .HasParameter("startDate").Metadata.TypeMapping = typeMappingSource.GetMapping("timestamp without time zone");
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(UDFSqlContext.GetCustomerWithMostOrdersAfterDateInstance)))
                .HasParameter("startDate").Metadata.TypeMapping = typeMappingSource.GetMapping("timestamp without time zone");
        }

        protected override void Seed(DbContext context)
        {
            base.Seed(context);

            context.Database.ExecuteSqlRaw(
                """
CREATE FUNCTION "CustomerOrderCount" ("customerId" INTEGER) RETURNS INTEGER
AS $$ SELECT COUNT("Id")::INTEGER FROM "Orders" WHERE "CustomerId" = $1 $$
LANGUAGE SQL;

CREATE FUNCTION "StarValue" ("starCount" INTEGER, value TEXT) RETURNS TEXT
AS $$ SELECT repeat('*', $1) || $2 $$
LANGUAGE SQL;

CREATE FUNCTION "StarValue" ("starCount" INTEGER, value INTEGER) RETURNS TEXT
AS $$ SELECT repeat('*', $1) || $2 $$
LANGUAGE SQL;

CREATE FUNCTION "DollarValue" ("starCount" INTEGER, value TEXT) RETURNS TEXT
AS $$ SELECT repeat('$', $1) || $2 $$
LANGUAGE SQL;

CREATE FUNCTION "GetReportingPeriodStartDate" (period INTEGER) RETURNS DATE
AS $$ SELECT DATE '1998-01-01' $$
LANGUAGE SQL;

CREATE FUNCTION "GetCustomerWithMostOrdersAfterDate" (searchDate TIMESTAMP) RETURNS INTEGER
AS $$
    SELECT "CustomerId"
    FROM "Orders"
    WHERE "OrderDate" > $1
    GROUP BY "CustomerId"
    ORDER BY COUNT("Id") DESC
    LIMIT 1
$$ LANGUAGE SQL;

CREATE FUNCTION "IsTopCustomer" ("customerId" INTEGER) RETURNS BOOL
AS $$ SELECT $1 = 1 $$
LANGUAGE SQL;

CREATE SCHEMA IF NOT EXISTS dbo;

CREATE FUNCTION dbo."IdentityString" ("customerName" TEXT) RETURNS TEXT
AS $$ SELECT $1 $$
LANGUAGE SQL;

CREATE FUNCTION "StringLength"("s" TEXT) RETURNS INT
AS $$ SELECT LENGTH($1) $$
LANGUAGE SQL;

CREATE FUNCTION "AddValues" (a INT, b INT) RETURNS INT
AS $$ SELECT $1 + $2 $$ LANGUAGE SQL;
""");

            context.SaveChanges();
        }
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
