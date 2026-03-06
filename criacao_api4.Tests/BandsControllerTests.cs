using criacao_api4.Controllers;
using criacao_api4.Models;
using criacao_api4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TesteUnitario;

[TestClass]
public class BandsControllerTests
{
    [TestMethod]
    public void GetAll_ShouldReturnOk()
    {
        using var db = CreateContext();
        db.Bands.Add(new Band { name = "A", description = "Desc A" });
        db.Bands.Add(new Band { name = "B", description = "Desc B" });
        db.SaveChanges();
        var controller = CreateController(db);

        var response = controller.GetAll();

        Assert.IsInstanceOfType(response.Result, typeof(OkObjectResult));
        var ok = (OkObjectResult)response.Result!;
        var bands = (List<Band>)ok.Value!;
        Assert.AreEqual(2, bands.Count);
    }

    [TestMethod]
    public void GetById_ShouldReturnNotFound_WhenBandDoesNotExist()
    {
        using var db = CreateContext();
        var controller = CreateController(db);

        var response = controller.GetById(100);

        Assert.IsInstanceOfType(response.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public void Create_ShouldReturnCreated_WhenBandIsValid()
    {
        using var db = CreateContext();
        var controller = CreateController(db);

        var response = controller.Create(new Band { name = "New Band", description = "Desc" });

        Assert.IsInstanceOfType(response.Result, typeof(CreatedAtActionResult));
    }

    [TestMethod]
    public void Create_ShouldReturnBadRequest_WhenBandNameIsInvalid()
    {
        using var db = CreateContext();
        var controller = CreateController(db);

        var response = controller.Create(new Band { name = " ", description = "Desc" });

        Assert.IsInstanceOfType(response.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public void Update_ShouldReturnNotFound_WhenBandDoesNotExist()
    {
        using var db = CreateContext();
        var controller = CreateController(db);

        var response = controller.Update(500, new Band { name = "X", description = "Y" });

        Assert.IsInstanceOfType(response.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public void Delete_ShouldReturnNoContent_WhenBandExists()
    {
        using var db = CreateContext();
        db.Bands.Add(new Band { name = "Delete Band", description = "Desc" });
        db.SaveChanges();
        var controller = CreateController(db);

        var response = controller.Delete(1);

        Assert.IsInstanceOfType(response, typeof(NoContentResult));
    }

    [TestMethod]
    public void GetWithCds_ShouldReturnNotFound_WhenBandDoesNotExist()
    {
        using var db = CreateContext();
        var controller = CreateController(db);

        var response = controller.GetWithCds(10);

        Assert.IsInstanceOfType(response.Result, typeof(NotFoundObjectResult));
    }

    private static BandsController CreateController(AppDbContext db)
    {
        var service = new BandServices(db, NullLogger<BandServices>.Instance);
        return new BandsController(service, NullLogger<BandsController>.Instance);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
