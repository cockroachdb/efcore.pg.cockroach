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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Metadata.Conventions;

/// <summary>
/// 
/// </summary>
public class CockroachPostgresModelFinalizingConvention : NpgsqlPostgresModelFinalizingConvention
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeMappingSource"></param>
    public CockroachPostgresModelFinalizingConvention(IRelationalTypeMappingSource typeMappingSource) : base(typeMappingSource)
    {
    }
    
    /// <summary>
    ///     Detects properties which are uint, OnAddOrUpdate and configured as concurrency tokens, and maps these to the PostgreSQL
    ///     internal "xmin" column, which changes every time the row is modified.
    /// </summary>
    protected override void ProcessRowVersionProperty(IConventionProperty property, RelationalTypeMapping typeMapping)
    {
        //
    }
}