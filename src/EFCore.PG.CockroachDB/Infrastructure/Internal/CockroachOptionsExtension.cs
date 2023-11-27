using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Infrastructure.Internal;

/// <summary>
/// 
/// </summary>
public class CockroachOptionsExtension : NpgsqlOptionsExtension
{
    /// <summary>
    /// 
    /// </summary>
    public CockroachOptionsExtension(): base()
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="copyFrom"></param>
    public CockroachOptionsExtension(CockroachOptionsExtension copyFrom)
        : base(copyFrom) {}
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public override void ApplyServices(IServiceCollection services) => services
        .AddEntityFrameworkNpgsql()
        .AddEntityFrameworkCockroach();
}