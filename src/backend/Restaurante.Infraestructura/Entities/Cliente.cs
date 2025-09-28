using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Cliente Entity
namespace Restaurante.Infraestructura.Entities
{
    public class Cliente
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        // Navigation: One-to-Many with Reservas
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        // Additional interesting properties
        public DateTime? FechaNacimiento { get; set; } // For birthday specials
        [StringLength(500)]
        public string? Preferencias { get; set; } // e.g., "Vegano, Mesa junto a ventana"
        public int PuntosLealtad { get; set; } = 0; // Loyalty points system
        public bool EsVip { get; set; } = false; // VIP status for priority
        public int NumeroVisitas { get; set; } = 0; // Visit count for analytics
        public string? NotasInternas { get; set; } // Staff notes, e.g., "Alergico a nueces"
    }
}
