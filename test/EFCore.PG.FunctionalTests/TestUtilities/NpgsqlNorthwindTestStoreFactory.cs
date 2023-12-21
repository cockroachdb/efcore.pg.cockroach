namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlNorthwindTestStoreFactory : NpgsqlTestStoreFactory
{
    public const string Name = "Northwind";
    public static readonly string NorthwindConnectionString = NpgsqlTestStore.CreateConnectionString(Name);
    public static new NpgsqlNorthwindTestStoreFactory Instance { get; } = new();

    static NpgsqlNorthwindTestStoreFactory()
    {
        // TODO: Switch to using NpgsqlDataSource
#pragma warning disable CS0618 // Type or member is obsolete
        NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
        NpgsqlConnection.GlobalTypeMapper.EnableRecordsAsTuples();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    protected NpgsqlNorthwindTestStoreFactory()
    {
    }

    public override TestStore GetOrCreate(string storeName)
        => NpgsqlTestStore.GetOrCreate(
            Name, "Northwind.sql", null);
}
