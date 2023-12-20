using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Infrastructure.Internal;

/// <summary>
/// 
/// </summary>
public class CockroachOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public void ApplyServices(IServiceCollection services) => services
        .AddEntityFrameworkCockroach();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Validate(IDbContextOptions options)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public DbContextOptionsExtensionInfo Info => _info ??= new CockroachOptionsExtensionInfo(this);
}

/// <summary>
/// 
/// </summary>
public class CockroachOptionsExtensionInfo : DbContextOptionsExtensionInfo
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="extension"></param>
    public CockroachOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override int GetServiceProviderHashCode() => 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="debugInfo"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public override bool IsDatabaseProvider => false;

    /// <summary>
    /// 
    /// </summary>
    public override string LogFragment => "CockroachOptionsExtension";
}