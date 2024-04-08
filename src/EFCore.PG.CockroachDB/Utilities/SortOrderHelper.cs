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

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Npgsql.EntityFrameworkCore.CockroachDB.Utilities;

internal static class SortOrderHelper
{
    public static bool IsDefaultNullSortOrder(
        IReadOnlyList<NullSortOrder>? nullSortOrders,
        IReadOnlyList<bool>? isDescendingValues)
    {
        if (nullSortOrders is null)
        {
            return true;
        }

        for (var i = 0; i < nullSortOrders.Count; i++)
        {
            var nullSortOrder = nullSortOrders[i];

            // We need to consider the ASC/DESC sort order to determine the default NULLS FIRST/LAST sort order.
            if (isDescendingValues is not null && (isDescendingValues.Count == 0 || isDescendingValues[i]))
            {
                // NULLS FIRST is the default when DESC is specified.
                if (nullSortOrder != NullSortOrder.NullsFirst)
                {
                    return false;
                }
            }
            else
            {
                // NULLS LAST is the default when DESC is NOT specified.
                if (nullSortOrder != NullSortOrder.NullsLast)
                {
                    return false;
                }
            }
        }

        return true;
    }
}