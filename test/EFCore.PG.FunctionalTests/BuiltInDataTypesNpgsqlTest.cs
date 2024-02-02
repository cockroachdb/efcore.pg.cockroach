﻿using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class BuiltInDataTypesNpgsqlTest : BuiltInDataTypesTestBase<BuiltInDataTypesNpgsqlTest.BuiltInDataTypesNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public BuiltInDataTypesNpgsqlTest(BuiltInDataTypesNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public void Sql_translation_uses_type_mapper_when_constant()
    {
        using var context = CreateContext();
        var results = context.Set<MappedNullableDataTypes>()
            .Where(e => e.TimeSpanAsTime == new TimeSpan(0, 1, 2))
            .Select(e => e.Int)
            .ToList();

        Assert.Empty(results);

        AssertSql(
            """
SELECT m."Int"
FROM "MappedNullableDataTypes" AS m
WHERE m."TimeSpanAsTime" = TIME '00:01:02'
""");
    }

    [Fact]
    public void Sql_translation_uses_type_mapper_when_parameter()
    {
        using var context = CreateContext();
        var timeSpan = new TimeSpan(2, 1, 0);
        var results = context.Set<MappedNullableDataTypes>()
            .Where(e => e.TimeSpanAsTime == timeSpan)
            .Select(e => e.Int)
            .ToList();

        Assert.Empty(results);

        AssertSql(
            """
@__timeSpan_0='02:01:00' (Nullable = true)

SELECT m."Int"
FROM "MappedNullableDataTypes" AS m
WHERE m."TimeSpanAsTime" = @__timeSpan_0
""");
    }

    [Fact]
    public virtual void Can_query_using_any_mapped_data_type()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(
                new MappedNullableDataTypes
                {
                    Int = 999,
                    LongAsBigint = 78L,
                    ShortAsSmallint = 79,
                    ByteAsSmallint = 80,
                    UintAsInt = uint.MaxValue,
                    UlongAsBigint = ulong.MaxValue,
                    UShortAsSmallint = ushort.MaxValue,
                    UintAsBigint = uint.MaxValue,
                    UShortAsInt = ushort.MaxValue,
                    BoolAsBoolean = true,
                    DecimalAsMoney = 81.1m,
                    Decimal = 101.7m,
                    DecimalAsNumeric = 103.9m,
                    BigIntegerAsNumeric = 105.4m,
                    FloatAsReal = 84.4f,
                    DoubleAsDoublePrecision = 85.5,
                    DateTimeAsTimestamp = new DateTime(2015, 1, 2, 10, 11, 12),
                    DateTimeAsTimestamptz = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc),
                    DateTimeAsDate = new DateTime(2015, 1, 2, 0, 0, 0),
                    TimeSpanAsTime = new TimeSpan(11, 15, 12),
                    DateTimeOffsetAsTimetz = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
                    TimeSpanAsInterval = new TimeSpan(11, 15, 12),
                    DateOnlyAsDate = new DateOnly(2015, 1, 2),
                    TimeOnlyAsTime = new TimeOnly(11, 15, 12),
                    StringAsText = "Gumball Rules!",
                    StringAsVarchar = "Gumball Rules OK",
                    // TODO: enable here (and above) after https://github.com/aspnet/EntityFrameworkCore/issues/14159 is fixed
                    // CharAsChar1 = 'f',
                    CharAsText = 'g',
                    CharAsVarchar = 'h',
                    BytesAsBytea = new byte[] { 86 },
                    GuidAsUuid = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
                    EnumAsText = StringEnum16.Value4,
                    EnumAsVarchar = StringEnumU16.Value4,
                    PhysicalAddressAsMacaddr = PhysicalAddress.Parse("08-00-2B-01-02-03"),
                    NpgsqlPointAsPoint = new NpgsqlPoint(5.2, 3.3),
                    StringAsJsonb = @"{""a"": ""b""}",
                    StringAsJson = @"{""a"": ""b""}",
                    DictionaryAsHstore = new Dictionary<string, string> { { "a", "b" } },
                    ImmutableDictionaryAsHstore = ImmutableDictionary<string, string>.Empty.Add("c", "d"),
                    NpgsqlRangeAsRange = new NpgsqlRange<int>(4, true, 8, false),
                    IntArrayAsIntArray = new[] { 2, 3 },
                    PhysicalAddressArrayAsMacaddrArray =
                        new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") },
                    UintAsXid = (uint)int.MaxValue + 1,

#pragma warning disable CS0618 // Full-text search client-parsing is obsolete
                    SearchQuery = NpgsqlTsQuery.Parse("a & b"),
                    SearchVector = NpgsqlTsVector.Parse("a b"),
#pragma warning restore CS0618
                    RankingNormalization = NpgsqlTsRankingNormalization.DivideByLength,
                    Regconfig = 12724,
                    Mood = Mood.Sad
                });

            Assert.Equal(1, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999);

            long? param1 = 78L;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.LongAsBigint == param1));

            short? param2 = 79;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ShortAsSmallint == param2));

            byte? param2a = 80;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ByteAsSmallint == param2a));

            uint? param3 = uint.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsInt == param3));

            ulong? param4 = ulong.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UlongAsBigint == param4));

            ushort? param5 = ushort.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UShortAsSmallint == param5));

            uint? param6 = uint.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsBigint == param6));

            ushort? param7 = ushort.MaxValue;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UShortAsInt == param7));

            bool? param8 = true;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BoolAsBoolean == param8));

            // PostgreSQL doesn't support comparing money to decimal
            //decimal? param9 = 81.1m;
            //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsMoney == param9));

            decimal? param10 = 101.7m;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Decimal == param10));

            decimal? param11 = 103.9m;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsNumeric == param11));

            decimal? param12a = 105.4m;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BigIntegerAsNumeric == param12a));

            float? param12 = 84.4f;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.FloatAsReal == param12));

            double? param13 = 85.5;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DoubleAsDoublePrecision == param13));

            DateTime? param14 = new DateTime(2015, 1, 2, 10, 11, 12);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsTimestamp == param14));

            DateTime? param15 = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsTimestamptz == param15));

            DateTime? param16 = new DateTime(2015, 1, 2, 0, 0, 0);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsDate == param16));

            TimeSpan? param17 = new TimeSpan(11, 15, 12);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.TimeSpanAsTime == param17));

            DateTimeOffset? param18 = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2));
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeOffsetAsTimetz == param18));

            TimeSpan? param19 = new TimeSpan(11, 15, 12);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.TimeSpanAsInterval == param19));

            DateOnly? param20 = new DateOnly(2015, 1, 2);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateOnlyAsDate == param20));

            TimeOnly? param21 = new TimeOnly(11, 15, 12);
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.TimeOnlyAsTime == param21));

            // ReSharper disable once ConvertToConstant.Local
            var param22 = "Gumball Rules!";
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsText == param22));

            // ReSharper disable once ConvertToConstant.Local
            var param23 = "Gumball Rules OK";
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsVarchar == param23));

            // TODO: enable here (and above) after https://github.com/aspnet/EntityFrameworkCore/issues/14159 is fixed
            // var param23a = 'f';
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsChar1 == param23a));

            // ReSharper disable once ConvertToConstant.Local
            var param23b = 'g';
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsText == param23b));

            // ReSharper disable once ConvertToConstant.Local
            var param23c = 'h';
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.CharAsVarchar == param23c));

            var param24 = new byte[] { 86 };
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BytesAsBytea == param24));

            Guid? param25 = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.GuidAsUuid == param25));

            StringEnum16? param26 = StringEnum16.Value4;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.EnumAsText == param26));

            StringEnumU16? param27 = StringEnumU16.Value4;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.EnumAsVarchar == param27));

            // CockroachDB doesn't support macaddr
            // var param28 = PhysicalAddress.Parse("08-00-2B-01-02-03");
            // Assert.Same(
            //    entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.PhysicalAddressAsMacaddr.Equals(param28)));

            // PostgreSQL doesn't support equality comparison on point
            // NpgsqlPoint? param29 = new NpgsqlPoint(5.2, 3.3);
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Point == param29));

            // ReSharper disable once ConvertToConstant.Local
            var param30 = @"{""a"": ""b""}";
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsJsonb == param30));

            // operator does not exist: json = json (https://stackoverflow.com/questions/32843213/operator-does-not-exist-json-json)
            // var param31 = @"{""a"": ""b""}";
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsJson == param31));

            // CockroachDB doesn't support hstore and range types
            // var param32 = new Dictionary<string, string> { { "a", "b" } };
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DictionaryAsHstore == param32));
            //
            // var param33 = ImmutableDictionary<string, string>.Empty.Add("c", "d");
            // Assert.Same(
            //     entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ImmutableDictionaryAsHstore == param33));
            //
            // var param34 = new NpgsqlRange<int>(4, true, 8, false);
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.NpgsqlRangeAsRange == param34));

            var param35 = new[] { 2, 3 };
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.IntArrayAsIntArray == param35));

            // CockroachDB doesn't support macaddr and xid
            // var param36 = new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") };
            // Assert.Same(
            //     entity,
            //     context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.PhysicalAddressArrayAsMacaddrArray == param36));
            //
            // // ReSharper disable once ConvertToConstant.Local
            // var param37 = (uint)int.MaxValue + 1;
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsXid == param37));

#pragma warning disable CS0618 // Full-text search client-parsing is obsolete
            var param38 = NpgsqlTsQuery.Parse("a & b");
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SearchQuery == param38));

            var param39 = NpgsqlTsVector.Parse("a b");
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SearchVector == param39));
#pragma warning restore CS0618

            // ReSharper disable once ConvertToConstant.Local
            var param40 = NpgsqlTsRankingNormalization.DivideByLength;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.RankingNormalization == param40));

            // ReSharper disable once ConvertToConstant.Local
            
            // var param41 = 12724u;
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Regconfig == param41));

            // ReSharper disable once ConvertToConstant.Local
            var param42 = Mood.Sad;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Mood == param42));
        }
    }

    [Fact]
    public virtual void Can_query_using_any_mapped_data_types_with_nulls()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(
                new MappedNullableDataTypes { Int = 911, });

            Assert.Equal(1, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911);

            long? param1 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.LongAsBigint == param1));

            short? param2 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ShortAsSmallint == param2));
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && (long?)e.ShortAsSmallint == param2));

            byte? param2a = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ByteAsSmallint == param2a));
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && (long?)e.ByteAsSmallint == param2a));

            uint? param3 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsInt == param3));

            ulong? param4 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UlongAsBigint == param4));

            ushort? param5 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsSmallint == param5));

            uint? param6 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsBigint == param6));

            ushort? param7 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsInt == param7));

            bool? param8 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BoolAsBoolean == param8));

            // CockroachDB doesn't support money type
            // decimal? param9 = null;
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsMoney == param9));

            decimal? param10 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Decimal == param10));

            decimal? param11 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsNumeric == param11));

            decimal? param111 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BigIntegerAsNumeric == param111));

            float? param12 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.FloatAsReal == param12));

            double? param13 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DoubleAsDoublePrecision == param13));

            DateTime? param14 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsTimestamp == param14));

            DateTime? param15 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsTimestamptz == param15));

            DateTime? param16 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDate == param16));

            TimeSpan? param17 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeSpanAsTime == param17));

            DateTimeOffset? param18 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeOffsetAsTimetz == param18));

            TimeSpan? param19 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeSpanAsInterval == param19));

            DateOnly? param20 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateOnlyAsDate == param20));

            TimeOnly? param21 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeOnlyAsTime == param21));

            string param22 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsText == param22));

            string param23 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsVarchar == param23));

            // TODO: enable here (and above) after https://github.com/aspnet/EntityFrameworkCore/issues/14159 is fixed
            // char? param23a = null;
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsChar1 == param23a));

            char? param23b = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsText == param23b));

            char? param23c = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsVarchar == param23c));

            byte[] param24 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsBytea == param24));

            Guid? param25 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.GuidAsUuid == param25));

            StringEnum16? param26 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.EnumAsText == param26));

            StringEnumU16? param27 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.EnumAsVarchar == param27));

            // PhysicalAddress param28 = null;
            // ReSharper disable once PossibleUnintendedReferenceComparison
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.PhysicalAddressAsMacaddr == param28));

            // PostgreSQL does not support equality comparison on geometry types, see https://www.postgresql.org/docs/current/functions-geometry.html
            //NpgsqlPoint? param29 = null;
            //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.NpgsqlPointAsPoint == param29));

            string param30 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsJsonb == param30));

            // PostgreSQL does not support equality comparison on json
            //string param31 = null;
            //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsJson == param31));

            // Dictionary<string, string> param32 = null;
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DictionaryAsHstore == param32));
            //
            // ImmutableDictionary<string, string> param33 = null;
            // Assert.Same(
            //     entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ImmutableDictionaryAsHstore == param33));
            //
            // NpgsqlRange<int>? param34 = null;
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.NpgsqlRangeAsRange == param34));

            int[] param35 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.IntArrayAsIntArray == param35));

            // PhysicalAddress[] param36 = null;
            // Assert.Same(
            //     entity,
            //     context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.PhysicalAddressArrayAsMacaddrArray == param36));
            //
            // uint? param37 = null;
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsXid == param37));

            NpgsqlTsQuery param38 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SearchQuery == param38));

            NpgsqlTsVector param39 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SearchVector == param39));

            NpgsqlTsRankingNormalization? param40 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.RankingNormalization == param40));

            // uint? param41 = null;
            // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Regconfig == param41));

            Mood? param42 = null;
            Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Mood == param42));
        }
    }

    [Fact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types()
    {
        var entity = CreateMappedDataTypes(77);
        using var context = CreateContext();
        context.Set<MappedDataTypes>().Add(entity);

        Assert.Equal(1, context.SaveChanges());

        var parameters = DumpParameters();
        Assert.Equal(
            @"@p0='77'
@p1='True'
@p2='80' (DbType = Int16)
@p3='0x56' (Nullable = false)
@p4='g' (Nullable = false)
@p5='h' (Nullable = false)
@p6='2015-01-02T00:00:00.0000000' (DbType = Date)
@p7='2015-01-02T10:11:12.0000000'
@p8='2016-01-02T11:11:12.0000000Z' (DbType = DateTime)
@p9='0001-01-01T12:00:00.0000000+02:00' (DbType = Object)
@p10='101.7'
@p11='103.9'
@p12='85.5'
@p13='Value4' (Nullable = false)
@p14='Value4' (Nullable = false)
@p15='84.4'
@p16='a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
@p17={ '2'
'3' } (Nullable = false) (DbType = Object)
@p18='78'
@p19='Sad' (DbType = Object)
@p20='2'
@p21=''a' & 'b'' (Nullable = false) (DbType = Object)
@p22=''a' 'b'' (Nullable = false) (DbType = Object)
@p23='79'
@p24='{""a"": ""b""}' (Nullable = false) (DbType = Object)
@p25='Gumball Rules!' (Nullable = false)
@p26='Gumball Rules OK' (Nullable = false)
@p27='11:15:12' (DbType = Object)
@p28='11:15:12'
@p29='65535'
@p30='-1'
@p31='4294967295'
@p32='-1'
@p33='-1'",
            parameters,
            ignoreLineEndingDifferences: true);
    }

    private string DumpParameters()
        => Fixture.TestSqlLoggerFactory.Parameters.Single().Replace(", ", Environment.NewLine);

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private static void AssertMappedDataTypes(MappedDataTypes entity, int id)
    {
        // ReSharper disable once UnusedVariable
        var expected = CreateMappedDataTypes(id);
        Assert.Equal(id, entity.Int);
        Assert.Equal(78, entity.LongAsBigint);
        Assert.Equal(79, entity.ShortAsSmallint);
        Assert.Equal(uint.MaxValue, entity.UintAsInt);
        Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
        Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
        Assert.Equal(uint.MaxValue, entity.UintAsBigint);
        Assert.Equal(ushort.MaxValue, entity.UShortAsInt);

        Assert.True(entity.BoolAsBoolean);

        Assert.Equal(81.1m, entity.DecimalAsMoney);
        Assert.Equal(101.7m, entity.Decimal);
        Assert.Equal(103.9m, entity.DecimalAsNumeric);
        Assert.Equal(84.4f, entity.FloatAsReal);
        Assert.Equal(85.5, entity.DoubleAsDoublePrecision);

        Assert.Equal(new DateTime(2015, 1, 2, 10, 11, 12), entity.DateTimeAsTimestamp);
        Assert.Equal(new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc), entity.DateTimeAsTimestamptz);
        Assert.Equal(new DateTime(2015, 1, 2, 0, 0, 0), entity.DateTimeAsDate);
        Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
        Assert.Equal(new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)), entity.DateTimeOffsetAsTimetz);
        Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsInterval);

        Assert.Equal("Gumball Rules!", entity.StringAsText);
        Assert.Equal("Gumball Rules OK", entity.StringAsVarchar);
        // TODO: enable here (and above) after https://github.com/aspnet/EntityFrameworkCore/issues/14159 is fixed
        // Assert.Equal('f', entity.CharAsChar1);
        Assert.Equal('g', entity.CharAsText);
        Assert.Equal('h', entity.CharAsVarchar);
        Assert.Equal(new byte[] { 86 }, entity.BytesAsBytea);

        Assert.Equal(new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"), entity.GuidAsUuid);

        Assert.Equal(StringEnum16.Value4, entity.EnumAsText);
        Assert.Equal(StringEnumU16.Value4, entity.EnumAsVarchar);

        Assert.Equal(PhysicalAddress.Parse("08-00-2B-01-02-03"), entity.PhysicalAddressAsMacaddr);
        Assert.Equal(new NpgsqlPoint(5.2, 3.3), entity.NpgsqlPointAsPoint);
        Assert.Equal(@"{""a"": ""b""}", entity.StringAsJsonb);
        Assert.Equal(@"{""a"": ""b""}", entity.StringAsJson);
        Assert.Equal(new Dictionary<string, string> { { "a", "b" } }, entity.DictionaryAsHstore);
        Assert.Equal(new NpgsqlRange<int>(4, true, 8, false), entity.NpgsqlRangeAsRange);

        Assert.Equal(new[] { 2, 3 }, entity.IntArrayAsIntArray);
        Assert.Equal(
            new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") },
            entity.PhysicalAddressArrayAsMacaddrArray);

        Assert.Equal((uint)int.MaxValue + 1, entity.UintAsXid);

#pragma warning disable CS0618 // Full-text search client-parsing is obsolete
        Assert.Equal(NpgsqlTsQuery.Parse("a & b").ToString(), entity.SearchQuery.ToString());
        Assert.Equal(NpgsqlTsVector.Parse("a b").ToString(), entity.SearchVector.ToString());
#pragma warning restore CS0618
        Assert.Equal(NpgsqlTsRankingNormalization.DivideByLength, entity.RankingNormalization);
    }

    private static MappedDataTypes CreateMappedDataTypes(int id)
        => new()
        {
            Int = id,
            LongAsBigint = 78L,
            ShortAsSmallint = 79,
            ByteAsSmallint = 80,
            UintAsInt = uint.MaxValue,
            UlongAsBigint = ulong.MaxValue,
            UShortAsSmallint = ushort.MaxValue,
            UintAsBigint = uint.MaxValue,
            UShortAsInt = ushort.MaxValue,
            BoolAsBoolean = true,
            DecimalAsMoney = 81.1m,
            Decimal = 101.7m,
            DecimalAsNumeric = 103.9m,
            FloatAsReal = 84.4f,
            DoubleAsDoublePrecision = 85.5,
            DateTimeAsTimestamp = new DateTime(2015, 1, 2, 10, 11, 12),
            DateTimeAsTimestamptz = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc),
            DateTimeAsDate = new DateTime(2015, 1, 2, 0, 0, 0),
            TimeSpanAsTime = new TimeSpan(11, 15, 12),
            DateTimeOffsetAsTimetz = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
            TimeSpanAsInterval = new TimeSpan(11, 15, 12),
            StringAsText = "Gumball Rules!",
            StringAsVarchar = "Gumball Rules OK",
            // TODO: enable here (and above) after https://github.com/aspnet/EntityFrameworkCore/issues/14159 is fixed
            // CharAsChar1 = 'f',
            CharAsText = 'g',
            CharAsVarchar = 'h',
            BytesAsBytea = new byte[] { 86 },
            GuidAsUuid = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
            EnumAsText = StringEnum16.Value4,
            EnumAsVarchar = StringEnumU16.Value4,
            PhysicalAddressAsMacaddr = PhysicalAddress.Parse("08-00-2B-01-02-03"),
            NpgsqlPointAsPoint = new NpgsqlPoint(5.2, 3.3),
            StringAsJsonb = @"{""a"": ""b""}",
            StringAsJson = @"{""a"": ""b""}",
            DictionaryAsHstore = new Dictionary<string, string> { { "a", "b" } },
            NpgsqlRangeAsRange = new NpgsqlRange<int>(4, true, 8, false),
            IntArrayAsIntArray = new[] { 2, 3 },
            PhysicalAddressArrayAsMacaddrArray =
                new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") },
            UintAsXid = (uint)int.MaxValue + 1,
#pragma warning disable CS0618 // Full-text search client-parsing is obsolete
            SearchQuery = NpgsqlTsQuery.Parse("a & b"),
            SearchVector = NpgsqlTsVector.Parse("a b"),
#pragma warning restore CS0618
            RankingNormalization = NpgsqlTsRankingNormalization.DivideByLength,
            Regconfig = 12724,
            Mood = Mood.Sad
        };

    [Fact]
    public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
    {
        using (var context = CreateContext())
        {
            context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 78 });

            Assert.Equal(1, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 78), 78);
        }
    }

    // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
    private static void AssertNullMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        // ReSharper restore ParameterOnlyUsedForPreconditionCheck.Local
    {
        Assert.Equal(id, entity.Int);
        Assert.Null(entity.LongAsBigint);
        Assert.Null(entity.ShortAsSmallint);
        Assert.Null(entity.ByteAsSmallint);
        Assert.Null(entity.UintAsInt);
        Assert.Null(entity.UlongAsBigint);
        Assert.Null(entity.UShortAsSmallint);
        Assert.Null(entity.UintAsBigint);
        Assert.Null(entity.UShortAsInt);

        Assert.Null(entity.BoolAsBoolean);

        Assert.Null(entity.DecimalAsMoney);
        Assert.Null(entity.Decimal);
        Assert.Null(entity.DecimalAsNumeric);
        Assert.Null(entity.FloatAsReal);
        Assert.Null(entity.DoubleAsDoublePrecision);

        Assert.Null(entity.DateTimeAsTimestamp);
        Assert.Null(entity.DateTimeAsTimestamptz);
        Assert.Null(entity.DateTimeAsDate);
        Assert.Null(entity.TimeSpanAsTime);
        Assert.Null(entity.DateTimeOffsetAsTimetz);
        Assert.Null(entity.TimeSpanAsInterval);

        Assert.Null(entity.DateOnlyAsDate);
        Assert.Null(entity.TimeOnlyAsTime);

        Assert.Null(entity.StringAsText);
        Assert.Null(entity.StringAsVarchar);
        // TODO: enable here (and above) after https://github.com/aspnet/EntityFrameworkCore/issues/14159 is fixed
        // Assert.Null(entity.CharAsChar1);
        Assert.Null(entity.CharAsText);
        Assert.Null(entity.CharAsVarchar);
        Assert.Null(entity.BytesAsBytea);

        Assert.Null(entity.GuidAsUuid);

        Assert.Null(entity.EnumAsText);
        Assert.Null(entity.EnumAsVarchar);

        Assert.Null(entity.PhysicalAddressAsMacaddr);
        Assert.Null(entity.NpgsqlPointAsPoint);
        Assert.Null(entity.StringAsJsonb);
        Assert.Null(entity.StringAsJson);
        Assert.Null(entity.DictionaryAsHstore);
        Assert.Null(entity.ImmutableDictionaryAsHstore);
        Assert.Null(entity.NpgsqlRangeAsRange);

        Assert.Null(entity.IntArrayAsIntArray);
        Assert.Null(entity.PhysicalAddressArrayAsMacaddrArray);

        Assert.Null(entity.UintAsXid);

        Assert.Null(entity.SearchQuery);
        Assert.Null(entity.SearchVector);
        Assert.Null(entity.RankingNormalization);

        Assert.Null(entity.Mood);
    }

    public override void Can_query_with_null_parameters_using_any_nullable_data_type()
    {
        using (var context = CreateContext())
        {
            context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes { Id = 711 });

            Assert.Equal(1, context.SaveChanges());
        }

        using (var context = CreateContext())
        {
            var entity = context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711).ToList().Single();

            short? param1 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt16 == param1).ToList().Single());
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && (long?)e.TestNullableInt16 == param1).ToList()
                    .Single());

            int? param2 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt32 == param2).ToList().Single());

            long? param3 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt64 == param3).ToList().Single());

            double? param4 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDouble == param4).ToList().Single());

            decimal? param5 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDecimal == param5).ToList().Single());

            DateTime? param6 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDateTime == param6).ToList().Single());

            // We don't support DateTimeOffset

            TimeSpan? param8 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableTimeSpan == param8).ToList().Single());

            float? param9 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableSingle == param9).ToList().Single());

            bool? param10 = null;
            Assert.Same(
                entity,
                context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableBoolean == param10).ToList().Single());

            // We don't support byte

            Enum64? param12 = null;
            Assert.Same(
                entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum64 == param12).ToList().Single());

            Enum32? param13 = null;
            Assert.Same(
                entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum32 == param13).ToList().Single());

            Enum16? param14 = null;
            Assert.Same(
                entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum16 == param14).ToList().Single());

            Enum8? param15 = null;
            Assert.Same(
                entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum8 == param15).ToList().Single());

            var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));
            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) is not null)
            {
                ushort? param16 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt16 == param16).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) is not null)
            {
                uint? param17 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt32 == param17).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) is not null)
            {
                ulong? param18 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt64 == param18).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) is not null)
            {
                char? param19 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableCharacter == param19).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) is not null)
            {
                sbyte? param20 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableSignedByte == param20).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) is not null)
            {
                EnumU64? param21 = null;
                Assert.Same(
                    entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU64 == param21).ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) is not null)
            {
                EnumU32? param22 = null;
                Assert.Same(
                    entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU32 == param22).ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) is not null)
            {
                EnumU16? param23 = null;
                Assert.Same(
                    entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU16 == param23).ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) is not null)
            {
                EnumS8? param24 = null;
                Assert.Same(
                    entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumS8 == param24).ToList().Single());
            }
        }
    }

    [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
    public override void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null() { }

    [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
    public override void Can_insert_and_read_back_non_nullable_backed_data_types() { }

    [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
    public override void Can_insert_and_read_back_nullable_backed_data_types() { }

    [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
    public override void Can_insert_and_read_back_object_backed_data_types() { }

    [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
    public override void Can_query_using_any_data_type_nullable_shadow() { }

    [ConditionalFact(Skip = "DateTimeOffset with non-zero offset, https://github.com/dotnet/efcore/issues/26068")]
    public override void Can_query_using_any_data_type_shadow() { }

    [ConditionalFact]
    public void Sum_Conversions()
    {
        using var context = CreateContext();

        // PostgreSQL SUM() returns numeric for bigint input, bigint for int/smallint ints.
        // Make sure the proper conversion is done
        _ = context.Set<MappedDataTypes>().Sum(m => m.LongAsBigint);
        _ = context.Set<MappedDataTypes>().Sum(m => m.Int);
        _ = context.Set<MappedDataTypes>().Sum(m => m.ShortAsSmallint);

        AssertSql(
            """
SELECT COALESCE(sum(m."LongAsBigint"), 0.0)::bigint
FROM "MappedDataTypes" AS m
""",
            //
            """
SELECT COALESCE(sum(m."Int"), 0)::int
FROM "MappedDataTypes" AS m
""",
            //
            """
SELECT COALESCE(sum(m."ShortAsSmallint"::int), 0)::int
FROM "MappedDataTypes" AS m
""");
    }

    [SkipForCockroachDb("CockroachDB doesn't support money type, https://github.com/cockroachdb/cockroach/issues/41578")]
    [ConditionalFact]
    public void Money_compare_constant()
    {
        using var context = CreateContext();

        _ = context.Set<MappedDataTypes>().Where(m => m.DecimalAsMoney > 3).ToList();
    }

    [SkipForCockroachDb("CockroachDB doesn't support money type, https://github.com/cockroachdb/cockroach/issues/41578")]
    [ConditionalFact]
    public void Money_compare_parameter()
    {
        using var context = CreateContext();

        var money = 3m;
        _ = context.Set<MappedDataTypes>().Where(m => m.DecimalAsMoney > money).ToList();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class BuiltInDataTypesNpgsqlFixture : BuiltInDataTypesFixtureBase
    {
        public override bool StrictEquality
            => false;

        public override bool SupportsAnsi
            => false;

        public override bool SupportsUnicodeToAnsiConversion
            => false;

        public override bool SupportsLargeStringComparisons
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override bool PreservesDateTimeKind
            => false;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Query.Name;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        static BuiltInDataTypesNpgsqlFixture()
        {
#pragma warning disable CS0618 // NpgsqlConnection.GlobalTypeMapper is obsolete
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Mood>();
#pragma warning restore CS0618
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // TODO: Switch to using data source
            ((NpgsqlTypeMappingSource)context.GetService<ITypeMappingSource>()).LoadUserDefinedTypeMappings(
                context.GetService<ISqlGenerationHelper>(), dataSource: null);

            modelBuilder.HasPostgresEnum("mood", new[] { "happy", "sad" });

            MakeRequired<MappedDataTypes>(modelBuilder);

            // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
            // supported.
            modelBuilder.Entity<BuiltInDataTypes>().Property(b => b.TestDateTime)
                .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<BuiltInNullableDataTypes>().Property(b => b.TestNullableDateTime)
                .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<BuiltInNullableDataTypesShadow>().Property(nameof(BuiltInNullableDataTypes.TestNullableDateTime))
                .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<ObjectBackedDataTypes>().Property(b => b.DateTime)
                .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<NullableBackedDataTypes>().Property(b => b.DateTime)
                .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<NonNullableBackedDataTypes>().Property(b => b.DateTime)
                .HasColumnType("timestamp without time zone");

            // We don't support DateTimeOffset with non-zero offset, so we need to override the seeding data
            var builtInDataTypes = modelBuilder.Entity<BuiltInDataTypes>().Metadata.GetSeedData().Single();
            builtInDataTypes[nameof(BuiltInDataTypes.TestDateTimeOffset)]
                = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.Zero);

            var builtInNullableDataTypes = modelBuilder.Entity<BuiltInNullableDataTypes>().Metadata.GetSeedData().Single();
            builtInNullableDataTypes[nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)]
                = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.Zero);

            var objectBackedDataTypes = modelBuilder.Entity<ObjectBackedDataTypes>().Metadata.GetSeedData().Single();
            objectBackedDataTypes[nameof(ObjectBackedDataTypes.DateTimeOffset)]
                = new DateTimeOffset(new DateTime(), TimeSpan.Zero);

            var nullableBackedDataTypes = modelBuilder.Entity<NullableBackedDataTypes>().Metadata.GetSeedData().Single();
            nullableBackedDataTypes[nameof(NullableBackedDataTypes.DateTimeOffset)]
                = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.Zero);

            var nonNullableBackedDataTypes = modelBuilder.Entity<NonNullableBackedDataTypes>().Metadata.GetSeedData().Single();
            nonNullableBackedDataTypes[nameof(NonNullableBackedDataTypes.DateTimeOffset)]
                = new DateTimeOffset(new DateTime(), TimeSpan.Zero);

            modelBuilder.Entity<BuiltInDataTypes>(
                b =>
                {
                    b.Ignore(dt => dt.TestUnsignedInt16);
                    b.Ignore(dt => dt.TestUnsignedInt32);
                    b.Ignore(dt => dt.TestUnsignedInt64);
                    b.Ignore(dt => dt.TestCharacter);
                    b.Ignore(dt => dt.TestSignedByte);
                    b.Ignore(dt => dt.TestDateTimeOffset);
                    b.Ignore(dt => dt.TestByte);
                    //b.Ignore(dt => dt.EnumU16);
                    //b.Ignore(dt => dt.EnumU32);
                    //b.Ignore(dt => dt.EnumU64);
                    //b.Ignore(dt => dt.EnumS8);
                });

            modelBuilder.Entity<BuiltInNullableDataTypes>(
                b =>
                {
                    b.Ignore(dt => dt.TestNullableUnsignedInt16);
                    b.Ignore(dt => dt.TestNullableUnsignedInt32);
                    b.Ignore(dt => dt.TestNullableUnsignedInt64);
                    b.Ignore(dt => dt.TestNullableCharacter);
                    b.Ignore(dt => dt.TestNullableSignedByte);
                    b.Ignore(dt => dt.TestNullableDateTimeOffset);
                    b.Ignore(dt => dt.TestNullableByte);
                    //b.Ignore(dt => dt.EnumU16);
                    //b.Ignore(dt => dt.EnumU32);
                    //b.Ignore(dt => dt.EnumU64);
                    //b.Ignore(dt => dt.EnumS8);
                });

            modelBuilder.Entity<MappedDataTypes>(
                b =>
                {
                    b.HasKey(e => e.Int);
                    b.Property(e => e.Int).ValueGeneratedNever();
                    
                    // Ignore fields that use data type unsupported by CockroachDB
                    b.Ignore(e => e.DictionaryAsHstore);
                    b.Ignore(e => e.DecimalAsMoney);
                    b.Ignore(e => e.PhysicalAddressAsMacaddr);
                    b.Ignore(e => e.NpgsqlPointAsPoint);
                    b.Ignore(e => e.PhysicalAddressArrayAsMacaddrArray);
                    b.Ignore(e => e.NpgsqlRangeAsRange);
                    b.Ignore(e => e.UintAsXid);
                    b.Ignore(e => e.Regconfig);
                    b.Ignore(e => e.StringAsJson);
                });

            modelBuilder.Entity<MappedNullableDataTypes>(
                b =>
                {
                    b.HasKey(e => e.Int);
                    b.Property(e => e.Int).ValueGeneratedNever();
                    
                    // Ignore fields that use data type unsupported by CockroachDB
                    b.Ignore(e => e.DictionaryAsHstore);
                    b.Ignore(e => e.ImmutableDictionaryAsHstore);
                    b.Ignore(e => e.DecimalAsMoney);
                    b.Ignore(e => e.PhysicalAddressAsMacaddr);
                    b.Ignore(e => e.NpgsqlPointAsPoint);
                    b.Ignore(e => e.PhysicalAddressArrayAsMacaddrArray);
                    b.Ignore(e => e.NpgsqlRangeAsRange);
                    b.Ignore(e => e.UintAsXid);
                    b.Ignore(e => e.Regconfig);
                    b.Ignore(e => e.StringAsJson);
                });

            modelBuilder.Entity<MappedSizedDataTypes>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<MappedScaledDataTypes>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            // Full text
            modelBuilder.Entity<MappedDataTypes>().Property(x => x.SearchQuery).HasColumnType("tsquery");
            modelBuilder.Entity<MappedDataTypes>().Property(x => x.SearchVector).HasColumnType("tsvector");
            modelBuilder.Entity<MappedDataTypes>().Property(x => x.RankingNormalization).HasColumnType("integer");
            
            // CockroachDB doesn't support regconfig type
            // modelBuilder.Entity<MappedDataTypes>().Property(x => x.Regconfig).HasColumnType("regconfig");
            modelBuilder.Entity<MappedNullableDataTypes>().Property(x => x.SearchQuery).HasColumnType("tsquery");
            modelBuilder.Entity<MappedNullableDataTypes>().Property(x => x.SearchVector).HasColumnType("tsvector");
            modelBuilder.Entity<MappedNullableDataTypes>().Property(x => x.RankingNormalization).HasColumnType("integer");
            // CockroachDB doesn't support regconfig type
            // modelBuilder.Entity<MappedNullableDataTypes>().Property(x => x.Regconfig).HasColumnType("regconfig");
        }

        public override bool SupportsBinaryKeys
            => true;

        public override DateTime DefaultDateTime
            => new();
    }

    protected enum StringEnum16 : short
    {
        // ReSharper disable once UnusedMember.Global
        Value1 = 1,

        // ReSharper disable once UnusedMember.Global
        Value2 = 2,
        Value4 = 4
    }

    protected enum StringEnumU16 : ushort
    {
        // ReSharper disable once UnusedMember.Global
        Value1 = 1,

        // ReSharper disable once UnusedMember.Global
        Value2 = 2,
        Value4 = 4
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected class MappedDataTypes
    {
        [Column(TypeName = "int")]
        public int Int { get; set; }

        [Column(TypeName = "bigint")]
        public long LongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public short ShortAsSmallint { get; set; }

        [Column(TypeName = "smallint")]
        public byte ByteAsSmallint { get; set; }

        [Column(TypeName = "int")]
        public uint UintAsInt { get; set; }

        [Column(TypeName = "bigint")]
        public uint UintAsBigint { get; set; }

        [Column(TypeName = "bigint")]
        public ulong UlongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public ushort UShortAsSmallint { get; set; }

        [Column(TypeName = "int")]
        public ushort UShortAsInt { get; set; }

        //[Column(TypeName = "tinyint")]
        //public sbyte SByteAsTinyint { get; set; }

        [Column(TypeName = "boolean")]
        public bool BoolAsBoolean { get; set; }

        [Column(TypeName = "numeric")]
        public decimal Decimal { get; set; } // decimal is just an alias for numeric

        [Column(TypeName = "numeric")]
        public decimal DecimalAsNumeric { get; set; }

        [Column(TypeName = "money")]
        public decimal DecimalAsMoney { get; set; }

        [Column(TypeName = "double precision")]
        public double DoubleAsDoublePrecision { get; set; }

        [Column(TypeName = "real")]
        public float FloatAsReal { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime DateTimeAsTimestamp { get; set; }

        [Column(TypeName = "timestamptz")]
        public DateTime DateTimeAsTimestamptz { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateTimeAsDate { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan TimeSpanAsTime { get; set; }

        [Column(TypeName = "timetz")]
        public DateTimeOffset DateTimeOffsetAsTimetz { get; set; }

        [Column(TypeName = "interval")]
        public TimeSpan TimeSpanAsInterval { get; set; }

        [Column(TypeName = "text")]
        public string StringAsText { get; set; }

        [Column(TypeName = "varchar")]
        public string StringAsVarchar { get; set; }

        // TODO: enable here (and above) after https://github.com/aspnet/EntityFrameworkCore/issues/14159 is fixed
        // [Column(TypeName = "char(1)")]
        // public char? CharAsChar1 { get; set; }

        [Column(TypeName = "text")]
        public char? CharAsText { get; set; }

        [Column(TypeName = "varchar")]
        public char? CharAsVarchar { get; set; }

        [Column(TypeName = "bytea")]
        public byte[] BytesAsBytea { get; set; }

        [Column(TypeName = "uuid")]
        public Guid GuidAsUuid { get; set; }

        [Column(TypeName = "text")]
        public StringEnum16 EnumAsText { get; set; }

        [Column(TypeName = "varchar")]
        public StringEnumU16 EnumAsVarchar { get; set; }

        // PostgreSQL-specific types from here

        [Column(TypeName = "macaddr")]
        public PhysicalAddress PhysicalAddressAsMacaddr { get; set; }

        [Column(TypeName = "point")]
        public NpgsqlPoint NpgsqlPointAsPoint { get; set; }

        [Column(TypeName = "jsonb")]
        public string StringAsJsonb { get; set; }

        [Column(TypeName = "json")]
        public string StringAsJson { get; set; }

        [Column(TypeName = "hstore")]
        public Dictionary<string, string> DictionaryAsHstore { get; set; }

        [Column(TypeName = "int4range")]
        public NpgsqlRange<int> NpgsqlRangeAsRange { get; set; }

        [Column(TypeName = "int[]")]
        public int[] IntArrayAsIntArray { get; set; }

        [Column(TypeName = "macaddr[]")]
        public PhysicalAddress[] PhysicalAddressArrayAsMacaddrArray { get; set; }

        [Column(TypeName = "xid")]
        public uint UintAsXid { get; set; }

        public NpgsqlTsQuery SearchQuery { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
        public NpgsqlTsRankingNormalization RankingNormalization { get; set; }
        public uint Regconfig { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [Column(TypeName = "mood")]
        public Mood Mood { get; set; }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class MappedSizedDataTypes
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }
        /*
        public string Char { get; set; }
        public string Character { get; set; }
        public string Varchar { get; set; }
        public string Char_varying { get; set; }
        public string Character_varying { get; set; }
        public string Nchar { get; set; }
        public string National_character { get; set; }
        public string Nvarchar { get; set; }
        public string National_char_varying { get; set; }
        public string National_character_varying { get; set; }
        public byte[] Binary { get; set; }
        public byte[] Varbinary { get; set; }
        public byte[] Binary_varying { get; set; }
        */
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class MappedScaledDataTypes
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }
        /*
        public float Float { get; set; }
        public float Double_precision { get; set; }
        public DateTimeOffset Datetimeoffset { get; set; }
        public DateTime Datetime2 { get; set; }
        public decimal Decimal { get; set; }
        public decimal Dec { get; set; }
        public decimal Numeric { get; set; }
        */
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class MappedPrecisionAndScaledDataTypes
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }
        /*
        public decimal Decimal { get; set; }
        public decimal Dec { get; set; }
        public decimal Numeric { get; set; }
        */
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected class MappedNullableDataTypes
    {
        [Column(TypeName = "int")]
        public int? Int { get; set; }

        [Column(TypeName = "bigint")]
        public long? LongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public short? ShortAsSmallint { get; set; }

        [Column(TypeName = "smallint")]
        public byte? ByteAsSmallint { get; set; }

        [Column(TypeName = "int")]
        public uint? UintAsInt { get; set; }

        [Column(TypeName = "bigint")]
        public uint? UintAsBigint { get; set; }

        [Column(TypeName = "bigint")]
        public ulong? UlongAsBigint { get; set; }

        [Column(TypeName = "smallint")]
        public ushort? UShortAsSmallint { get; set; }

        [Column(TypeName = "int")]
        public ushort? UShortAsInt { get; set; }

        //[Column(TypeName = "tinyint")]
        //public sbyte? SByteAsTinyint { get; set; }

        [Column(TypeName = "boolean")]
        public bool? BoolAsBoolean { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Decimal { get; set; } // decimal is just an alias for numeric

        [Column(TypeName = "numeric")]
        public decimal? DecimalAsNumeric { get; set; }

        [Column(TypeName = "money")]
        public decimal? DecimalAsMoney { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? BigIntegerAsNumeric { get; set; }

        [Column(TypeName = "double precision")]
        public double? DoubleAsDoublePrecision { get; set; }

        [Column(TypeName = "real")]
        public float? FloatAsReal { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime? DateTimeAsTimestamp { get; set; }

        [Column(TypeName = "timestamptz")]
        public DateTime? DateTimeAsTimestamptz { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateTimeAsDate { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan? TimeSpanAsTime { get; set; }

        [Column(TypeName = "date")]
        public DateOnly? DateOnlyAsDate { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly? TimeOnlyAsTime { get; set; }

        [Column(TypeName = "timetz")]
        public DateTimeOffset? DateTimeOffsetAsTimetz { get; set; }

        [Column(TypeName = "interval")]
        public TimeSpan? TimeSpanAsInterval { get; set; }

        [Column(TypeName = "text")]
        public string StringAsText { get; set; }

        [Column(TypeName = "varchar")]
        public string StringAsVarchar { get; set; }

        // TODO: enable here (and above) after https://github.com/aspnet/EntityFrameworkCore/issues/14159 is fixed
        // [Column(TypeName = "char(1)")]
        // public char? CharAsChar1 { get; set; }

        [Column(TypeName = "text")]
        public char? CharAsText { get; set; }

        [Column(TypeName = "varchar")]
        public char? CharAsVarchar { get; set; }

        [Column(TypeName = "bytea")]
        public byte[] BytesAsBytea { get; set; }

        [Column(TypeName = "uuid")]
        public Guid? GuidAsUuid { get; set; }

        [Column(TypeName = "text")]
        public StringEnum16? EnumAsText { get; set; }

        [Column(TypeName = "varchar")]
        public StringEnumU16? EnumAsVarchar { get; set; }

        // PostgreSQL-specific types from here

        [Column(TypeName = "macaddr")]
        public PhysicalAddress PhysicalAddressAsMacaddr { get; set; }

        [Column(TypeName = "point")]
        public NpgsqlPoint? NpgsqlPointAsPoint { get; set; }

        [Column(TypeName = "jsonb")]
        public string StringAsJsonb { get; set; }

        [Column(TypeName = "json")]
        public string StringAsJson { get; set; }

        [Column(TypeName = "hstore")]
        public Dictionary<string, string> DictionaryAsHstore { get; set; }

        [Column(TypeName = "hstore")]
        public ImmutableDictionary<string, string> ImmutableDictionaryAsHstore { get; set; }

        [Column(TypeName = "int4range")]
        public NpgsqlRange<int>? NpgsqlRangeAsRange { get; set; }

        [Column(TypeName = "int[]")]
        public int[] IntArrayAsIntArray { get; set; }

        [Column(TypeName = "macaddr[]")]
        public PhysicalAddress[] PhysicalAddressArrayAsMacaddrArray { get; set; }

        [Column(TypeName = "xid")]
        public uint? UintAsXid { get; set; }

        public NpgsqlTsQuery SearchQuery { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
        public NpgsqlTsRankingNormalization? RankingNormalization { get; set; }
        public uint? Regconfig { get; set; }

        [Column(TypeName = "mood")]
        public Mood? Mood { get; set; }
    }
}

// ReSharper disable once UnusedMember.Global
public enum Mood { Happy, Sad }
