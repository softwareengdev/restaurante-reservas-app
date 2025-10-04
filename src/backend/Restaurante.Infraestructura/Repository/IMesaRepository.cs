using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Specific repository interfaces in Restaurante.Infraestructura.Repository
// For Mesa
namespace Restaurante.Infraestructura.Repository
{
    public interface IMesaRepository : IBaseRepository<Entities.Mesa>
    {
        // Add any Mesa-specific data access methods if needed
        Task<bool> ExistsByNumeroAsync(string numero);
    }
}
