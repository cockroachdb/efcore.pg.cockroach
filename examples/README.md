### Install [Npgsql Entity Framework Core provider for PostgreSQL](https://github.com/npgsql/efcore.pg) package

```
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Set the connection string

Example connection string

```
Host=localhost;Port=26257;Username=username;Password=password;Database=CockroachEFCoreExample
```

For more information on how to construct a connection string, see this [page](https://www.npgsql.org/doc/connection-string-parameters.html)

Set a DATABASE_CONNECTION environment variable to your connection string.

```
export DATABASE_CONNECTION="{connection string}"
```

### Run the basic example

Compile and run the code:

```
dotnet run --project src/CockroachEFCoreExample Simple
```

The output should be

```
Initial balances:
        account 1: 1000
        account 2: 250
Final balances:
        account 1: 1500
        account 3: 5000

```

### Run the transactions example

This time, running the code will execute a batch of statements as an atomic transaction to transfer funds from one account to another, where all included statements are either committed or aborted:

```
dotnet run --project src/CockroachEFCoreExample Transaction
```

The output should be:

```
Initial balances:
        account 1: 1000
        account 2: 250
Final balances:
        account 1: 900
        account 2: 350

```