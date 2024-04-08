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

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace System.Reflection;

internal static class TypeExtensions
{
    internal static bool IsGenericList(this Type? type)
        => type is not null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

    internal static bool IsArrayOrGenericList(this Type type)
        => type.IsArray || type.IsGenericList();

    internal static bool TryGetElementType(this Type type, [NotNullWhen(true)] out Type? elementType)
    {
        elementType = type.IsArray
            ? type.GetElementType()
            : type.IsGenericList()
                ? type.GetGenericArguments()[0]
                : null;
        return elementType is not null;
    }

    internal static bool IsRange(this SqlExpression expression)
        => expression.TypeMapping is NpgsqlRangeTypeMapping || expression.Type.IsRange();

    internal static bool IsRange(this Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(NpgsqlRange<>)
            || type is { Name: "Interval" or "DateInterval", Namespace: "NodaTime" };

    internal static bool IsMultirange(this SqlExpression expression)
        => expression.TypeMapping is NpgsqlMultirangeTypeMapping
            || expression.Type.IsMultirange();

    internal static bool IsMultirange(this Type type)
        => type.TryGetElementType(out var elementType) && elementType.IsRange();

    public static PropertyInfo? FindIndexerProperty(this Type type)
    {
        var defaultPropertyAttribute = type.GetCustomAttributes<DefaultMemberAttribute>().FirstOrDefault();

        return defaultPropertyAttribute is null
            ? null
            : type.GetRuntimeProperties()
                .FirstOrDefault(pi => pi.Name == defaultPropertyAttribute.MemberName && pi.GetIndexParameters().Length == 1);
    }
}
