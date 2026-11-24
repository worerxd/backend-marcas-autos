using MarcasAutos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Api.Data;

public class MarcasAutosDbContext : DbContext
{
    public MarcasAutosDbContext(
        DbContextOptions<MarcasAutosDbContext> options)
        : base(options)
    {
    }

    public DbSet<MarcaAuto> MarcasAutos => Set<MarcaAuto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MarcaAuto>(entity =>
        {
            entity.ToTable("MarcasAutos");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(m => m.PaisOrigen)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasData(
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
        });

        
    }
}