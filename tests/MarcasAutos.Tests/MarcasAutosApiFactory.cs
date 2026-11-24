using MarcasAutos.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MarcasAutos.Tests;

public sealed class MarcasAutosApiFactory : WebApplicationFactory<Program>
{
    private readonly string _environmentName;
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public MarcasAutosApiFactory(string environmentName)
    {
        _environmentName = environmentName;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environmentName);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<MarcasAutosDbContext>>();
            services.RemoveAll<DbContextOptions<MarcasAutosDbContext>>();
            services.RemoveAll<MarcasAutosDbContext>();

            var dbContextOptions = services
                .Where(service => service.ServiceType == typeof(DbContextOptions))
                .ToList();

            foreach (var descriptor in dbContextOptions)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<MarcasAutosDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(_environmentName);

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MarcasAutosDbContext>();
        context.Database.EnsureCreated();

        return host;
    }
}
