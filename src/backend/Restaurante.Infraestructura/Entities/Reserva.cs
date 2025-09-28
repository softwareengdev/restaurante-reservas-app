using Restaurante.Infraestructura.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Reserva Entity
namespace Restaurante.Infraestructura.Entities
{
    public class Reserva
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MesaId { get; set; }

        [ForeignKey(nameof(MesaId))]
        public virtual Mesa? Mesa { get; set; }

        [Required]
        public Guid ClienteId { get; set; }

        [ForeignKey(nameof(ClienteId))]
        public virtual Cliente? Cliente { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; } // Start date and time

        [Required]
        public TimeSpan Duracion { get; set; } // Duration, e.g., TimeSpan.FromHours(2)

        public string Estado { get; set; } = "Pendiente"; // "Pendiente", "Confirmada", "Cancelada", "Completada"

        [StringLength(500)]
        public string? Notas { get; set; } // Special requests, e.g., "Cumpleaños"

        // Additional interesting properties
        public int NumeroPersonas { get; set; } // Actual number attending (<= Mesa.Capacidad)
        public bool RequiereMenuEspecial { get; set; } = false; // e.g., Vegetarian, Allergies
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // Reservation creation timestamp
        public DateTime? FechaCancelacion { get; set; } // If cancelled
    }
}
