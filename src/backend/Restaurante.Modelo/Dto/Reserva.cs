using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Reserva DTOs
namespace Restaurante.Modelo.Dto
{
    public class ReservaDto
    {
        public Guid Id { get; set; }
        public Guid MesaId { get; set; }
        public Guid ClienteId { get; set; }
        public DateTime FechaInicio { get; set; }
        public TimeSpan Duracion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public int NumeroPersonas { get; set; }
        public bool RequiereMenuEspecial { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCancelacion { get; set; }
    }

    public class CreateReservaDto
    {
        [Required]
        public Guid MesaId { get; set; }
        [Required]
        public Guid ClienteId { get; set; }
        [Required]
        public DateTime FechaInicio { get; set; }
        [Required]
        public TimeSpan Duracion { get; set; }
        public string? Notas { get; set; }
        [Range(1, int.MaxValue)]
        public int NumeroPersonas { get; set; }
        public bool RequiereMenuEspecial { get; set; } = false;
    }

    public class UpdateReservaDto : CreateReservaDto
    {
        public string Estado { get; set; } = "Pendiente"; // Allow updating status
    }
}
