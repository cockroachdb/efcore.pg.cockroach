// Copyright (c) 2002-2021, Npgsql
// 
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written agreement
// is hereby granted, provided that the above copyright notice and this
// paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL NPGSQL BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT,
// SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS,
// ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF
// Npgsql HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// NPGSQL SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED
// TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS ON AN "AS IS" BASIS, AND Npgsql
// HAS NO OBLIGATIONS TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS,
// OR MODIFICATIONS.
//

using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Scaffolding.Internal;

internal static class DbDataReaderExtension
{
    [DebuggerStepThrough]
    [return: MaybeNull]
    internal static T GetValueOrDefault<T>(this DbDataReader reader, string name)
    {
        var idx = reader.GetOrdinal(name);
        return reader.IsDBNull(idx)
            ? default
            : reader.GetFieldValue<T>(idx);
    }

    [DebuggerStepThrough]
    [return: MaybeNull]
    internal static T GetValueOrDefault<T>(this DbDataRecord record, string name)
    {
        var idx = record.GetOrdinal(name);
        return record.IsDBNull(idx)
            ? default
            : (T)record.GetValue(idx);
    }

    [DebuggerStepThrough]
    internal static T GetFieldValue<T>(this DbDataRecord record, string name)
        => (T)record.GetValue(record.GetOrdinal(name));
}