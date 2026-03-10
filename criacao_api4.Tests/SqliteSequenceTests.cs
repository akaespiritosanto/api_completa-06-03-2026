using criacao_api4.Models;
using criacao_api4.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TesteUnitario;

[TestClass]
public class SqliteSequenceTests
{
    [TestMethod]
    public void Bands_DeleteLastAndCreate_ShouldReuseId()
    {
        using var db = SqliteTestDb.Create();
        var service = new BandServices(db.Context, NullLogger<BandServices>.Instance);

        var first = service.Create(new Band { name = "A", description = "A" });
        Assert.AreEqual(1, first.bandId);

        var deleted = service.Delete(first.bandId);
        Assert.IsTrue(deleted);

        var second = service.Create(new Band { name = "B", description = "B" });
        Assert.AreEqual(1, second.bandId);
    }

    [TestMethod]
    public void Cds_DeleteLastAndCreate_ShouldReuseId()
    {
        using var db = SqliteTestDb.Create();
        var bandService = new BandServices(db.Context, NullLogger<BandServices>.Instance);
        var cdService = new CdServices(db.Context, NullLogger<CdServices>.Instance);

        var band = bandService.Create(new Band { name = "Band", description = "Desc" });
        var first = cdService.Create(new Cd { name = "CD 1", bandId = band.bandId, rating = 5 });
        Assert.AreEqual(1, first.cdId);

        var deleted = cdService.Delete(first.cdId);
        Assert.IsTrue(deleted);

        var second = cdService.Create(new Cd { name = "CD 2", bandId = band.bandId, rating = 5 });
        Assert.AreEqual(1, second.cdId);
    }

    private sealed class SqliteTestDb : IDisposable
    {
        public required SqliteConnection Connection { get; init; }
        public required AppDbContext Context { get; init; }

        public static SqliteTestDb Create()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            return new SqliteTestDb { Connection = connection, Context = context };
        }

        public void Dispose()
        {
            Context.Dispose();
            Connection.Dispose();
        }
    }
}
