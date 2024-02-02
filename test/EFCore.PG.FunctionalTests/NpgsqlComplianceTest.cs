﻿namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
    {
        // Not implemented
        typeof(FromSqlSprocQueryTestBase<>),
        typeof(UdfDbFunctionTestBase<>),
        typeof(UpdateSqlGeneratorTestBase),
        
        // Disabled
        typeof(GraphUpdatesTestBase<>),
        typeof(ProxyGraphUpdatesTestBase<>),
        typeof(OperatorsProceduralQueryTestBase),
        
        typeof(OptimisticConcurrencyTestBase<,>),
        typeof(OptimisticConcurrencyRelationalTestBase<,>),
        typeof(TransactionInterceptionTestBase),
        typeof(TransactionTestBase<>),
    };

    protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
}
