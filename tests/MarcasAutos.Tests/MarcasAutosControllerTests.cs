
using MarcasAutos.Api.Controllers;
using MarcasAutos.Api.Data;
using MarcasAutos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Tests;

public class MarcasAutosControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsAllBrands()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MarcasAutosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MarcasAutosDbContext(options);

        context.MarcasAutos.AddRange(
            new MarcaAuto
            {
                Id = 1,
                Nombre = "Toyota",
                PaisOrigen = "Japón"
            },
            new MarcaAuto
            {
                Id = 2,
                Nombre = "BMW",
                PaisOrigen = "Alemania"
            },
            new MarcaAuto
            {
                Id = 3,
                Nombre = "Ford",
                PaisOrigen = "Estados Unidos"
            }
        );

        await context.SaveChangesAsync();

        var controller = new MarcasAutosController(context);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var marcas = Assert.IsAssignableFrom<IEnumerable<MarcaAuto>>(
            okResult.Value
        );

        Assert.Equal(3, marcas.Count());
        Assert.Contains(marcas, m => m.Nombre == "Toyota");
        Assert.Contains(marcas, m => m.Nombre == "BMW");
        Assert.Contains(marcas, m => m.Nombre == "Ford");
    }

    [Fact]
    public async Task GetAll_WhenNoBrandsExist_ReturnsEmptyCollection()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MarcasAutosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MarcasAutosDbContext(options);

        var controller = new MarcasAutosController(context);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var marcas = Assert.IsAssignableFrom<IEnumerable<MarcaAuto>>(
            okResult.Value
        );

        Assert.Empty(marcas);
    }
}
