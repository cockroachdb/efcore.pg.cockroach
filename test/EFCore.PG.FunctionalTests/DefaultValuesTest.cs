﻿using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class DefaultValuesTest : IDisposable
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddEntityFrameworkNpgsql()
        .BuildServiceProvider();

    [Fact]
    public void Can_use_Npgsql_default_values()
    {
        using (var context = new ChipsContext(_serviceProvider, "DefaultKettleChips"))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var honeyDijon = context.Add(new KettleChips { Name = "Honey Dijon" }).Entity;
            var buffaloBleu = context.Add(
                new KettleChips { Name = "Buffalo Bleu", BestBuyDate = new DateTime(2111, 1, 11, 0, 0, 0, DateTimeKind.Utc) }).Entity;

            context.SaveChanges();

            Assert.Equal(new DateTime(2035, 9, 25, 0, 0, 0, DateTimeKind.Utc), honeyDijon.BestBuyDate);
            Assert.Equal(new DateTime(2111, 1, 11, 0, 0, 0, DateTimeKind.Utc), buffaloBleu.BestBuyDate);
        }

        using (var context = new ChipsContext(_serviceProvider, "DefaultKettleChips"))
        {
            Assert.Equal(
                new DateTime(2035, 9, 25, 0, 0, 0, DateTimeKind.Utc), context.Chips.Single(c => c.Name == "Honey Dijon").BestBuyDate);
            Assert.Equal(
                new DateTime(2111, 1, 11, 0, 0, 0, DateTimeKind.Utc), context.Chips.Single(c => c.Name == "Buffalo Bleu").BestBuyDate);
        }
    }

    public void Dispose()
    {
        using var context = new ChipsContext(_serviceProvider, "DefaultKettleChips");
        context.Database.EnsureDeleted();
    }

    private class ChipsContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _databaseName;

        public ChipsContext(IServiceProvider serviceProvider, string databaseName)
        {
            _serviceProvider = serviceProvider;
            _databaseName = databaseName;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<KettleChips> Chips { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseNpgsql(
                    NpgsqlTestStore.CreateConnectionString(_databaseName),
                    o => o.SetPostgresVersion(TestEnvironment.PostgresVersion))
                .UseInternalServiceProvider(_serviceProvider);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<KettleChips>()
                .Property(e => e.BestBuyDate)
                .ValueGeneratedOnAdd()
                .HasDefaultValue(new DateTime(2035, 9, 25, 0, 0, 0, DateTimeKind.Utc));
    }

    private class KettleChips
    {
        // ReSharper disable once UnusedMember.Local
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime BestBuyDate { get; set; }
    }
}
