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

using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace CockroachEfCoreExample;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");
        if (connectionString == null)
        {
            var connStringBuilder = new NpgsqlConnectionStringBuilder();
            connStringBuilder.SslMode = SslMode.Prefer;
            connStringBuilder.Host = "localhost";
            connStringBuilder.Port = 26257;
            connStringBuilder.Username = "username";
            connStringBuilder.Password = "password";
            connStringBuilder.Database = "Bank";

            connectionString = connStringBuilder.ConnectionString;
        }

        await using var ctx = new BankContext(connectionString);
        await ctx.Database.EnsureCreatedAsync();

        // Clean-up Accounts table
        await ctx.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Accounts\"");

        var example = args.FirstOrDefault();
        if (example != "Transaction")
        {
            await SimpleExample.Run(connectionString);
        }
        else
        {
            await TransactionExample.Run(connectionString);
        }
    }
}