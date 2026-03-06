using criacao_api4.Controllers;
using criacao_api4.Models;
using criacao_api4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TesteUnitario;

[TestClass]
public class CdsControllerTests
{
    [TestMethod]
    public void GetAll_ShouldReturnOk()
    {
        using var db = CreateContext();
        var band = new Band { name = "Band", description = "Desc" };
        db.Bands.Add(band);
        db.SaveChanges();
        db.Cds.Add(new Cd { name = "CD 1", bandId = band.bandId, rating = 4 });
        db.Cds.Add(new Cd { name = "CD 2", bandId = band.bandId, rating = 5 });
        db.SaveChanges();
        var controller = CreateController(db);

        var response = controller.GetAll();

        Assert.IsInstanceOfType(response.Result, typeof(OkObjectResult));
        var ok = (OkObjectResult)response.Result!;
        var cds = (List<Cd>)ok.Value!;
        Assert.AreEqual(2, cds.Count);
    }

    [TestMethod]
    public void GetById_ShouldReturnNotFound_WhenCdDoesNotExist()
    {
        using var db = CreateContext();
        var controller = CreateController(db);

        var response = controller.GetById(200);

        Assert.IsInstanceOfType(response.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public void GetByName_ShouldReturnBadRequest_WhenNameIsBlank()
    {
        using var db = CreateContext();
        var controller = CreateController(db);

        var response = controller.GetByName(" ");

        Assert.IsInstanceOfType(response.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public void Create_ShouldReturnCreated_WhenCdIsValid()
    {
        using var db = CreateContext();
        var band = new Band { name = "Band", description = "Desc" };
        db.Bands.Add(band);
        db.SaveChanges();
        var controller = CreateController(db);

        var response = controller.Create(new Cd { name = "CD", bandId = band.bandId, rating = 5 });

        Assert.IsInstanceOfType(response.Result, typeof(CreatedAtActionResult));
    }

    [TestMethod]
    public void Update_ShouldReturnBadRequest_WhenRatingIsInvalid()
    {
        using var db = CreateContext();
        var band = new Band { name = "Band", description = "Desc" };
        db.Bands.Add(band);
        db.SaveChanges();
        db.Cds.Add(new Cd { name = "CD", bandId = band.bandId, rating = 3 });
        db.SaveChanges();
        var controller = CreateController(db);

        var response = controller.Update(1, new Cd { name = "CD", bandId = band.bandId, rating = 6 });

        Assert.IsInstanceOfType(response.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public void Delete_ShouldReturnNotFound_WhenCdDoesNotExist()
    {
        using var db = CreateContext();
        var controller = CreateController(db);

        var response = controller.Delete(10);

        Assert.IsInstanceOfType(response, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public void GetByBand_ShouldReturnOk()
    {
        using var db = CreateContext();
        var band = new Band { name = "Band", description = "Desc" };
        db.Bands.Add(band);
        db.SaveChanges();
        db.Cds.Add(new Cd { name = "CD", bandId = band.bandId, rating = 5 });
        db.SaveChanges();
        var controller = CreateController(db);

        var response = controller.GetByBand(band.bandId);

        Assert.IsInstanceOfType(response.Result, typeof(OkObjectResult));
        var ok = (OkObjectResult)response.Result!;
        var cds = (List<Cd>)ok.Value!;
        Assert.AreEqual(1, cds.Count);
    }

    private static CdsController CreateController(AppDbContext db)
    {
        var service = new CdServices(db, NullLogger<CdServices>.Instance);
        return new CdsController(service, NullLogger<CdsController>.Instance);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
