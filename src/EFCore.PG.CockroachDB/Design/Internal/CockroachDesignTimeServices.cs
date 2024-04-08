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

using Microsoft.EntityFrameworkCore.Design.Internal;
using Npgsql.EntityFrameworkCore.CockroachDB.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Design.Internal;

/// <summary>
/// 
/// </summary>
public class CockroachDesignTimeServices : NpgsqlDesignTimeServices
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceCollection"></param>
    public override void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        Check.NotNull(serviceCollection, nameof(serviceCollection));

        serviceCollection.AddEntityFrameworkCockroach();
        
        new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
            .TryAdd<ICSharpRuntimeAnnotationCodeGenerator, NpgsqlCSharpRuntimeAnnotationCodeGenerator>()
            .TryAdd<IAnnotationCodeGenerator, NpgsqlAnnotationCodeGenerator>()
            .TryAdd<IDatabaseModelFactory, CockroachDatabaseModelFactory>()
            .TryAdd<IProviderConfigurationCodeGenerator, NpgsqlCodeGenerator>()
            .TryAddCoreServices();
    }
}