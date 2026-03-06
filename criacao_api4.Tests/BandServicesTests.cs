using criacao_api4.Models;
using criacao_api4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TesteUnitario;

[TestClass]
public class BandServicesTests
{
    [TestMethod]
    public void Create_ShouldSaveBand()
    {
        using var db = CreateContext();
        var service = new BandServices(db, NullLogger<BandServices>.Instance);
        var band = new Band { name = "Metallica", description = "Thrash metal" };

        var createdBand = service.Create(band);

        Assert.IsTrue(createdBand.bandId > 0);
        Assert.AreEqual(1, db.Bands.Count());
        Assert.AreEqual("Metallica", db.Bands.Single().name);
    }

    [TestMethod]
    public void Create_ShouldThrow_WhenNameIsEmpty()
    {
        using var db = CreateContext();
        var service = new BandServices(db, NullLogger<BandServices>.Instance);
        var band = new Band { name = " " };

        Assert.ThrowsException<ArgumentException>(() => service.Create(band));
    }

    [TestMethod]
    public void GetWithCds_ShouldReturnBandAndCds()
    {
        using var db = CreateContext();
        var band = new Band { name = "Nirvana", description = "Grunge" };
        db.Bands.Add(band);
        db.SaveChanges();

        db.Cds.Add(new Cd { name = "In Utero", bandId = band.bandId, rating = 5 });
        db.Cds.Add(new Cd { name = "Nevermind", bandId = band.bandId, rating = 5 });
        db.SaveChanges();

        var service = new BandServices(db, NullLogger<BandServices>.Instance);
        var result = service.GetWithCds(band.bandId);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(BandWithCds));
        var bandWithCds = (BandWithCds)result;
        Assert.AreEqual("Nirvana", bandWithCds.name);
        Assert.AreEqual(2, bandWithCds.cds.Count);
    }

    [TestMethod]
    public void GetById_ShouldReturnNull_WhenBandDoesNotExist()
    {
        using var db = CreateContext();
        var service = new BandServices(db, NullLogger<BandServices>.Instance);

        var result = service.GetById(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Update_ShouldChangeBand_WhenBandExists()
    {
        using var db = CreateContext();
        db.Bands.Add(new Band { name = "Old", description = "Old description" });
        db.SaveChanges();
        var service = new BandServices(db, NullLogger<BandServices>.Instance);

        var result = service.Update(1, new Band { name = "New", description = "New description" });

        Assert.IsNotNull(result);
        Assert.AreEqual("New", result.name);
        Assert.AreEqual("New description", result.description);
    }

    [TestMethod]
    public void Update_ShouldReturnNull_WhenBandDoesNotExist()
    {
        using var db = CreateContext();
        var service = new BandServices(db, NullLogger<BandServices>.Instance);

        var result = service.Update(50, new Band { name = "Name", description = "Desc" });

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Delete_ShouldReturnTrue_WhenBandExists()
    {
        using var db = CreateContext();
        db.Bands.Add(new Band { name = "Band", description = "Desc" });
        db.SaveChanges();
        var service = new BandServices(db, NullLogger<BandServices>.Instance);

        var deleted = service.Delete(1);

        Assert.IsTrue(deleted);
        Assert.AreEqual(0, db.Bands.Count());
    }

    [TestMethod]
    public void Delete_ShouldReturnFalse_WhenBandDoesNotExist()
    {
        using var db = CreateContext();
        var service = new BandServices(db, NullLogger<BandServices>.Instance);

        var deleted = service.Delete(1);

        Assert.IsFalse(deleted);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
