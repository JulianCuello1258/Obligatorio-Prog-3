using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Apiario> Apiarios { get; set; }
        public DbSet<Colmena> Colmenas { get; set; }
        public DbSet<Reina> Reinas { get; set; }
        public DbSet<Revision> Revisiones { get; set; }
        public DbSet<Extraccion> Extracciones { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Trashumancia> Trashumancias { get; set; }
        public DbSet<Exportacion> Exportaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Reina>()
                .HasKey(r => r.ColmenaId);

            modelBuilder.Entity<Colmena>()
                .HasOne(c => c.Reina)
                .WithOne(r => r.Colmena)
                .HasForeignKey<Reina>(r => r.ColmenaId);

            modelBuilder.Entity<Trashumancia>()
                .HasOne(t => t.ApiarioOrigen)
                .WithMany()
                .HasForeignKey(t => t.ApiarioOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trashumancia>()
                .HasOne(t => t.ApiarioDestino)
                .WithMany()
                .HasForeignKey(t => t.ApiarioDestinoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enum conversions
            modelBuilder.Entity<Apiario>()
                .Property(a => a.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<Colmena>()
                .Property(c => c.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<Revision>()
                .Property(r => r.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<Colmena>()
                .Property(c => c.Poblacion)
                .HasConversion<string>();

            modelBuilder.Entity<Revision>()
                .Property(r => r.PoblacionEstimada)
                .HasConversion<string>();

            modelBuilder.Entity<Colmena>()
                .Property(c => c.Temperamento)
                .HasConversion<string>();

            modelBuilder.Entity<Revision>()
                .Property(r => r.Temperamento)
                .HasConversion<string>();

            modelBuilder.Entity<Reina>()
                .Property(r => r.Salud)
                .HasConversion<string>();

            modelBuilder.Entity<Revision>()
                .Property(r => r.ReinaSalud)
                .HasConversion<string>();

            modelBuilder.Entity<Revision>()
                .Property(r => r.NivelInfestacion)
                .HasConversion<string>();

            // Value Objects
            modelBuilder.Entity<Revision>()
                .OwnsOne(r => r.CondicionesClimaticas, cb =>
                {
                    cb.ToTable("Clima");
                    cb.Property(c => c.Temperatura).HasColumnName("Temperatura");
                    cb.Property(c => c.Humedad).HasColumnName("Humedad");
                    cb.Property(c => c.Presion).HasColumnName("Presion");
                    cb.Property(c => c.VelocidadViento).HasColumnName("VelocidadViento");
                    cb.Property(c => c.DireccionViento).HasColumnName("DireccionViento");
                });
        }
    }
}
