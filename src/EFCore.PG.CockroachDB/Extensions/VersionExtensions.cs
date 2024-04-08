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

namespace System.Reflection;

internal static class VersionExtensions
{
    // Note: a null version is interpreted as the latest version and will always return true.
    internal static bool AtLeast(this Version? version, int major, int minor = 0)
        => version is null || version >= new Version(major, minor);

    // Note: a null version is interpreted as the latest version and will always return false.
    internal static bool IsUnder(this Version? version, int major, int minor = 0)
        => version is not null && version < new Version(major, minor);
}