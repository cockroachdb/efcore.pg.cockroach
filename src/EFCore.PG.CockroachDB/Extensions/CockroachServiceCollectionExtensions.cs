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

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.CockroachDB.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.CockroachDB.Migrations;
using Npgsql.EntityFrameworkCore.CockroachDB.Storage.Internal;

namespace System.Reflection;

/// <summary>
/// 
/// </summary>
public static class CockroachServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddEntityFrameworkCockroach(this IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        var serviceDescriptors = serviceCollection
            .Where(descriptor => descriptor.ServiceType == typeof(IRelationalTypeMappingSource) || 
                                 descriptor.ServiceType == typeof(IRelationalDatabaseCreator) ||
                                 descriptor.ServiceType == typeof(IProviderConventionSetBuilder) ||
                                 descriptor.ServiceType == typeof(IMigrationsSqlGenerator))
            .ToList();
        
        foreach (var descriptor in serviceDescriptors)
        {
            serviceCollection.Remove(descriptor);
        }
        
        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<IRelationalTypeMappingSource, CockroachTypeMappingSource>()
            .TryAdd<IMigrationsSqlGenerator, CockroachMigrationsSqlGenerator>()
            .TryAdd<IProviderConventionSetBuilder, CockroachConventionSetBuilder>()
            .TryAdd<IRelationalDatabaseCreator, CockroachDatabaseCreator>();
        
        return serviceCollection;
    }
}