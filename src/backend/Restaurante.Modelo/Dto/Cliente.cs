using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Cliente DTOs
namespace Restaurante.Modelo.Dto
{
    public class ClienteDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string? Preferencias { get; set; }
        public int PuntosLealtad { get; set; }
        public bool EsVip { get; set; }
        public int NumeroVisitas { get; set; }
        public string? NotasInternas { get; set; }
    }

    public class CreateClienteDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string Apellidos { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string Telefono { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string? Preferencias { get; set; }
        public bool EsVip { get; set; } = false;
        public string? NotasInternas { get; set; }
    }

    public class UpdateClienteDto : CreateClienteDto
    {
        // Add optional fields for partial updates
    }
}