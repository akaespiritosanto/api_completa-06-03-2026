using criacao_api4.Models;
using criacao_api4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TesteUnitario;

[TestClass]
public class CdServicesTests
{
    [TestMethod]
    public void Create_ShouldThrow_WhenRatingIsInvalid()
    {
        using var db = CreateContext();
        var band = new Band { name = "Queen", description = "Rock" };
        db.Bands.Add(band);
        db.SaveChanges();

        var service = new CdServices(db, NullLogger<CdServices>.Instance);
        var cd = new Cd { name = "A Night at the Opera", bandId = band.bandId, rating = 6 };

        Assert.ThrowsException<ArgumentException>(() => service.Create(cd));
    }

    [TestMethod]
    public void Create_ShouldThrow_WhenBandDoesNotExist()
    {
        using var db = CreateContext();
        var service = new CdServices(db, NullLogger<CdServices>.Instance);
        var cd = new Cd { name = "Unknown", bandId = 999, rating = 4 };

        Assert.ThrowsException<ArgumentException>(() => service.Create(cd));
    }

    [TestMethod]
    public void GetByName_ShouldFindCd()
    {
        using var db = CreateContext();
        var band = new Band { name = "Linkin Park", description = "Nu metal" };
        db.Bands.Add(band);
        db.SaveChanges();

        db.Cds.Add(new Cd { name = "Hybrid Theory", bandId = band.bandId, rating = 5 });
        db.Cds.Add(new Cd { name = "Meteora", bandId = band.bandId, rating = 5 });
        db.SaveChanges();

        var service = new CdServices(db, NullLogger<CdServices>.Instance);
        var result = service.GetByName("  THEORY ");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Hybrid Theory", result[0].name);
    }

    [TestMethod]
    public void GetById_ShouldReturnNull_WhenCdDoesNotExist()
    {
        using var db = CreateContext();
        var service = new CdServices(db, NullLogger<CdServices>.Instance);

        var result = service.GetById(999);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetByBand_ShouldReturnOnlyBandCds()
    {
        using var db = CreateContext();
        var band1 = new Band { name = "Band 1", description = "Desc" };
        var band2 = new Band { name = "Band 2", description = "Desc" };
        db.Bands.Add(band1);
        db.Bands.Add(band2);
        db.SaveChanges();
        db.Cds.Add(new Cd { name = "CD 1", bandId = band1.bandId, rating = 4 });
        db.Cds.Add(new Cd { name = "CD 2", bandId = band2.bandId, rating = 5 });
        db.SaveChanges();
        var service = new CdServices(db, NullLogger<CdServices>.Instance);

        var result = service.GetByBand(band1.bandId);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("CD 1", result[0].name);
    }

    [TestMethod]
    public void GetByName_ShouldReturnEmptyList_WhenNameIsBlank()
    {
        using var db = CreateContext();
        var service = new CdServices(db, NullLogger<CdServices>.Instance);

        var result = service.GetByName(" ");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Update_ShouldChangeCd_WhenCdExists()
    {
        using var db = CreateContext();
        var band = new Band { name = "Band", description = "Desc" };
        db.Bands.Add(band);
        db.SaveChanges();
        db.Cds.Add(new Cd { name = "Old CD", bandId = band.bandId, rating = 2 });
        db.SaveChanges();
        var service = new CdServices(db, NullLogger<CdServices>.Instance);

        var result = service.Update(1, new Cd { name = "New CD", bandId = band.bandId, rating = 5 });

        Assert.IsNotNull(result);
        Assert.AreEqual("New CD", result.name);
        Assert.AreEqual(5, result.rating);
    }

    [TestMethod]
    public void Delete_ShouldReturnTrue_WhenCdExists()
    {
        using var db = CreateContext();
        var band = new Band { name = "Band", description = "Desc" };
        db.Bands.Add(band);
        db.SaveChanges();
        db.Cds.Add(new Cd { name = "CD", bandId = band.bandId, rating = 4 });
        db.SaveChanges();
        var service = new CdServices(db, NullLogger<CdServices>.Instance);

        var deleted = service.Delete(1);

        Assert.IsTrue(deleted);
        Assert.AreEqual(0, db.Cds.Count());
    }

    [TestMethod]
    public void Delete_ShouldReturnFalse_WhenCdDoesNotExist()
    {
        using var db = CreateContext();
        var service = new CdServices(db, NullLogger<CdServices>.Instance);

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
