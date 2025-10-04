using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// DTOs for API (place in Restaurante.Api.Models or Dtos folder)
// Use separate DTOs for Create, Update, Read to follow best practices

// Mesa DTOs
namespace Restaurante.Modelo.Dto
{
    public class MesaDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public bool EsAccesible { get; set; }
        public bool TieneVista { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? UltimaLimpieza { get; set; }
        public int CalificacionPromedio { get; set; }
        public virtual ICollection<ReservaDto> Reservas { get; set; } = new List<ReservaDto>();
    }

    public class CreateMesaDto
    {
        [Required]
        public string Numero { get; set; } = string.Empty;
        [Required]
        [Range(1, 20)]
        public int Capacidad { get; set; }
        public string Ubicacion { get; set; } = "Interior";
        public bool EsAccesible { get; set; } = false;
        public bool TieneVista { get; set; } = false;
        public string Estado { get; set; } = "Disponible";
    }

    public class UpdateMesaDto : CreateMesaDto
    {
        // Add fields that can be updated, e.g., exclude Id
    }
}