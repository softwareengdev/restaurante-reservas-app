using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// First, add or update entities in your domain/infra layer (e.g., Restaurante.Infraestructura.Entities)
// We'll use Fluent API in DbContext for configurations, including relationships and data types.

// Mesa Entity
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurante.Infraestructura.Entities
{
    public class Mesa
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Numero { get; set; } = string.Empty; // e.g., "Mesa 1"

        [Required]
        [Range(1, 20)] // Assuming max 20 people per table
        public int Capacidad { get; set; } // Key property: number of people

        [StringLength(100)]
        public string Ubicacion { get; set; } = "Interior"; // e.g., "Interior", "Terraza", "Privado"

        public bool EsAccesible { get; set; } = false; // Wheelchair accessible

        public bool TieneVista { get; set; } = false; // Scenic view

        public string Estado { get; set; } = "Disponible"; // "Disponible", "Ocupada", "Mantenimiento"

        // Navigation: One-to-Many with Reservas
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        // Additional interesting properties
        public DateTime? UltimaLimpieza { get; set; } // Last cleaning time for hygiene tracking
        public int CalificacionPromedio { get; set; } = 0; // Average rating from users (1-5)
    }
}