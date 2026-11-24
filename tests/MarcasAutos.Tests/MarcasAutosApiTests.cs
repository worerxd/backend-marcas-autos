using System.Net;
using System.Net.Http.Json;
using MarcasAutos.Api.Models;

namespace MarcasAutos.Tests;

public sealed class MarcasAutosApiTests : IClassFixture<MarcasAutosApiHosts>
{
    private static readonly string[] ForecastSummaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ];

    private readonly MarcasAutosApiHosts _hosts;

    public MarcasAutosApiTests(MarcasAutosApiHosts hosts)
    {
        _hosts = hosts;
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task GetMarcasAutos_ReturnsSeededCatalog(string environmentName)
    {
        using var client = _hosts.CreateClient(environmentName);

        var response = await client.GetAsync("/api/marcasautos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var marcas = await response.Content.ReadFromJsonAsync<List<MarcaAuto>>();

        Assert.NotNull(marcas);
        Assert.Equal(3, marcas.Count);
        Assert.Contains(marcas, marca => marca.Nombre == "Toyota" && marca.PaisOrigen == "Japón");
        Assert.Contains(marcas, marca => marca.Nombre == "BMW" && marca.PaisOrigen == "Alemania");
        Assert.Contains(marcas, marca => marca.Nombre == "Ford" && marca.PaisOrigen == "Estados Unidos");
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task GetWeatherForecast_ReturnsFiveForecasts(string environmentName)
    {
        using var client = _hosts.CreateClient(environmentName);

        var response = await client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var forecasts = await response.Content.ReadFromJsonAsync<List<WeatherForecastResponse>>();

        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Count);
        Assert.All(forecasts, forecast =>
        {
            Assert.Contains(forecast.Summary, ForecastSummaries);
            Assert.Equal(
                32 + (int)(forecast.TemperatureC / 0.5556),
                forecast.TemperatureF);
        });
    }

    [Fact]
    public async Task OpenApiDocument_IsAvailableInDevelopment()
    {
        using var client = _hosts.CreateClient("Development");

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"openapi\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenApiDocument_IsNotMappedInProduction()
    {
        using var client = _hosts.CreateClient("Production");

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record WeatherForecastResponse(
        DateOnly Date,
        int TemperatureC,
        string? Summary,
        int TemperatureF);
}

public sealed class MarcasAutosApiHosts : IDisposable
{
    public MarcasAutosApiFactory Development { get; } = new("Development");

    public MarcasAutosApiFactory Production { get; } = new("Production");

    public HttpClient CreateClient(string environmentName)
    {
        var factory = environmentName switch
        {
            "Development" => Development,
            "Production" => Production,
            _ => throw new ArgumentOutOfRangeException(
                nameof(environmentName),
                environmentName,
                "El entorno de prueba debe ser Development o Production.")
        };

        return factory.CreateClient();
    }

    public void Dispose()
    {
        Development.Dispose();
        Production.Dispose();
    }
}
