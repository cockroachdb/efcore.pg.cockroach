using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlTestHelpers : RelationalTestHelpers
{
    protected NpgsqlTestHelpers() { }

    public static NpgsqlTestHelpers Instance { get; } = new();

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkNpgsql().AddEntityFrameworkCockroach();

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(new NpgsqlConnection("Host=localhost;Database=DummyDatabase")).UseCockroach();

    public override LoggingDefinitions LoggingDefinitions { get; } = new NpgsqlLoggingDefinitions();
}
