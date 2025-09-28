using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Restaurante.Modelo.Entities;
using Restaurante.Modelo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurante.Infraestructura.Entities;

namespace Restaurante.Infraestructura.DBContext
{
    public class RestauranteDbContext : IdentityDbContext<ApplicationUser>
    {
        // DbSets — ajusta o añade según tus entidades reales del proyecto Restaurante.Entities

        // In ApplicationDbContext, add:
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }


        public RestauranteDbContext(DbContextOptions<RestauranteDbContext> options)
            : base(options)
        {
        }
        // Add other DbSets here if needed

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Custom configurations, e.g., seed roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "User", NormalizedName = "USER" }
            );

            // Configurations for relationships and constraints
            builder.Entity<Mesa>()
                .HasMany(m => m.Reservas)
                .WithOne(r => r.Mesa)
                .HasForeignKey(r => r.MesaId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            builder.Entity<Cliente>()
                .HasMany(c => c.Reservas)
                .WithOne(r => r.Cliente)
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Cascade); // Delete reservations if client deleted (adjust as needed)

            // Indexes for performance
            builder.Entity<Reserva>()
                .HasIndex(r => r.FechaInicio)
                .HasDatabaseName("IX_Reservas_FechaInicio");

            builder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_Clientes_Email");

            // Seed sample data (optional for initial migration)
            builder.Entity<Mesa>().HasData(
                new Mesa { Id = Guid.NewGuid(), Numero = "Mesa 1", Capacidad = 4, Ubicacion = "Interior" }
            );
        }
    }
}
