# CockroachDB extension for Npgsql.EntityFrameworkCore.PostgreSQL

This provider is currently in beta. For a list of known limitations see docs/known-limitations.md
This package should be consumed via [nuget](https://www.nuget.org/packages/CockroachDB.EFCore.Provider).

Npgsql.EntityFrameworkCore.CockroachDB is an extension library to add compatibility for CockroachDB to the open source EF Core provider for PostgreSQL. It allows you to interact with CockroachDB via the most widely-used .NET O/RM from Microsoft, and use familiar LINQ syntax to express queries. It's built on top of [Npgsql.EntityFrameworkCore.PostgreSQL](https://github.com/npgsql/efcore.pg).

Here's a quick sample to get you started:

```csharp
await using var ctx = new BlogContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

// Insert a Blog
ctx.Blogs.Add(new() { Name = "FooBlog" });
await ctx.SaveChangesAsync();

// Query all blogs who's name starts with F
var fBlogs = await ctx.Blogs.Where(b => b.Name.StartsWith("F")).ToListAsync();

public class BlogContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseNpgsql(@"Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase")
            .UseCockroach();
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```