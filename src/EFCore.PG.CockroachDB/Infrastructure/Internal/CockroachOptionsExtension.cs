// Copyright 2024 The Cockroach Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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
        return true;
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