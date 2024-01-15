// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipForCockroachDbAttribute(string reason = null) : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync() => ValueTask.FromResult(false);

    public string SkipReason => string.IsNullOrWhiteSpace(reason) ? "Skip for CockroachDB" : $"Skip for CockroachDB: {reason}";
}
