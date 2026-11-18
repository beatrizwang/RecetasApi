using Microsoft.EntityFrameworkCore;
using RecetasApi.Models;

namespace RecetasApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Alimento> Alimentos => Set<Alimento>();
    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<RecetaAlimento> RecetaAlimentos => Set<RecetaAlimento>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alimento>(e =>
        {
            e.Property(a => a.Nombre).HasMaxLength(100).UseCollation("NOCASE");
            e.HasIndex(a => a.Nombre).IsUnique();

            // Enums guardados como texto: más legibles en la BD y robustos si se reordena el enum.
            e.Property(a => a.Categoria).HasConversion<string>().HasMaxLength(30);

            e.HasData(SeedData.Alimentos);
        });

        modelBuilder.Entity<Receta>(e =>
        {
            e.Property(r => r.Nombre).HasMaxLength(150);
            e.Property(r => r.Descripcion).HasMaxLength(1000);
            e.Property(r => r.Dificultad).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<RecetaAlimento>(e =>
        {
            e.HasKey(x => new { x.RecetaId, x.AlimentoId });
            e.Property(x => x.Unidad).HasConversion<string>().HasMaxLength(20);

            // Al borrar una receta se borran sus líneas de ingredientes.
            e.HasOne(x => x.Receta)
                .WithMany(r => r.Ingredientes)
                .HasForeignKey(x => x.RecetaId)
                .OnDelete(DeleteBehavior.Cascade);

            // No se puede borrar un alimento que esté en alguna receta.
            e.HasOne(x => x.Alimento)
                .WithMany(a => a.Recetas)
                .HasForeignKey(x => x.AlimentoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ahora = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaCreacion = ahora;
                entry.Entity.FechaActualizacion = ahora;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaActualizacion = ahora;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
