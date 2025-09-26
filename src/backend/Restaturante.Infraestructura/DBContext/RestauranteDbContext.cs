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

namespace Restaturante.Infraestructura.DBContext
{
    public class RestauranteDbContext : IdentityDbContext<ApplicationUser>
    {
        // DbSets — ajusta o añade según tus entidades reales del proyecto Restaurante.Entities

        // In ApplicationDbContext, add:
        public DbSet<RefreshToken> RefreshTokens { get; set; }

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
        }
    }
}
