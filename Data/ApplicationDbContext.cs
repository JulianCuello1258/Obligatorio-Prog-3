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
        }
    }
}
