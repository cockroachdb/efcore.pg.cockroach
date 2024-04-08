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

using System.Collections;
using System.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Storage.Internal;

/// <summary>
/// 
/// </summary>
public class CockroachTypeMappingSource : NpgsqlTypeMappingSource
{
    private readonly NpgsqlStringTypeMapping _varchar = new("character varying", NpgsqlDbType.Varchar);
    private readonly NpgsqlCharacterStringTypeMapping _char = new("character");
    private readonly NpgsqlCharacterCharTypeMapping _singleChar = new("character(1)");
    private readonly NpgsqlBitTypeMapping _bit = NpgsqlBitTypeMapping.Default;
    private readonly NpgsqlVarbitTypeMapping _varbit = NpgsqlVarbitTypeMapping.Default;
    
    private readonly StringTypeMapping _text = new("text", DbType.String);
    private readonly NpgsqlByteArrayTypeMapping _bytea = NpgsqlByteArrayTypeMapping.Default;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="sqlGenerationHelper"></param>
    /// <param name="options"></param>
    public CockroachTypeMappingSource(
        TypeMappingSourceDependencies dependencies, 
        RelationalTypeMappingSourceDependencies relationalDependencies, 
        ISqlGenerationHelper sqlGenerationHelper, 
        INpgsqlSingletonOptions options) : base(dependencies, relationalDependencies, sqlGenerationHelper, options)
    {
        StoreTypeMappings["json"] = StoreTypeMappings["jsonb"];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mappingInfo"></param>
    /// <returns></returns>
    protected override RelationalTypeMapping? FindBaseMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;
        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

        if (storeTypeName is not null)
        {
            if (StoreTypeMappings.TryGetValue(storeTypeName, out var mappings))
            {
                // We found the user-specified store type. No CLR type was provided - we're probably
                // scaffolding from an existing database, take the first mapping as the default.
                if (clrType is null)
                {
                    return mappings[0];
                }

                // A CLR type was provided - look for a mapping between the store and CLR types. If not found, fail
                // immediately.
                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                // Map arbitrary user POCOs to JSON
                if (storeTypeName is "jsonb" or "json")
                {
                    return new NpgsqlJsonTypeMapping("jsonb", clrType);
                }

                return null;
            }

            if (StoreTypeMappings.TryGetValue(storeTypeNameBase!, out mappings))
            {
                if (clrType is null)
                {
                    return mappings[0];
                }

                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                return null;
            }

            // 'character' is special: 'character' (no size) and 'character(1)' map to a single char, whereas 'character(n)' maps
            // to a string
            if (storeTypeNameBase is "character" or "char")
            {
                if (mappingInfo.Size is null or 1 && clrType is null || clrType == typeof(char))
                {
                    return _singleChar.Clone(mappingInfo);
                }

                if (clrType is null || clrType == typeof(string))
                {
                    return _char.Clone(mappingInfo);
                }
            }

            // TODO: the following is a workaround/hack for https://github.com/dotnet/efcore/issues/31505
            if ((storeTypeName.EndsWith("[]", StringComparison.Ordinal)
                    || storeTypeName is "int4multirange" or "int8multirange" or "nummultirange" or "datemultirange" or "tsmultirange"
                        or "tstzmultirange")
                && FindCollectionMapping(mappingInfo, mappingInfo.ClrType!, providerType: null, elementMapping: null) is
                    RelationalTypeMapping collectionMapping)
            {
                return collectionMapping;
            }

            // A store type name was provided, but is unknown. This could be a domain (alias) type, in which case
            // we proceed with a CLR type lookup (if the type doesn't exist at all the failure will come later).
        }

        if (clrType is not null)
        {
            if (ClrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                // Handle types with the size facet (string, bitarray)
                if (mappingInfo.Size is > 0)
                {
                    if (clrType == typeof(string))
                    {
                        mapping = mappingInfo.IsFixedLength ?? false ? _char : _varchar;

                        // See #342 for when size > 10485760
                        return mappingInfo.Size <= 10485760
                            ? mapping.WithStoreTypeAndSize($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size)
                            : _text;
                    }

                    if (clrType == typeof(BitArray))
                    {
                        mapping = mappingInfo.IsFixedLength ?? false ? _bit : _varbit;
                        return mapping.WithStoreTypeAndSize($"{mapping.StoreType}({mappingInfo.Size})", mappingInfo.Size);
                    }
                }

                return mapping;
            }

            if (clrType == typeof(byte[]) && mappingInfo.ElementTypeMapping is null)
            {
                if (storeTypeName == "smallint[]")
                {
                    // PostgreSQL has no tinyint (single-byte) type, but we allow mapping CLR byte to PG smallint (2-bytes).
                    // The same applies to arrays - as always - so byte[] should be mappable to smallint[].
                    // However, byte[] also has a base mapping to bytea, which is the default. So when the user explicitly specified
                    // mapping to smallint[], we don't return that to allow the array mapping to work.
                    // TODO: This is a workaround; RelationalTypeMappingSource first attempts to find a value converter before trying
                    // to find a collection. We should reverse the order and call FindCollectionMapping before attempting to find a
                    // value converter.
                    // TODO: Make sure the providerType should be null
                    return FindCollectionMapping(mappingInfo, typeof(byte[]), providerType: null, elementMapping: null);
                    // return null;
                }

                return _bytea;
            }

            if (mappingInfo.IsRowVersion == true)
            {
                return null;
            }
        }

        return null;
    }
}