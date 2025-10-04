using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// For Reserva
namespace Restaurante.Infraestructura.Repository
{
    public interface IReservaRepository : IBaseRepository<Entities.Reserva>
    {
        Task<bool> HasConflictAsync(Guid mesaId, DateTime start, DateTime end, Guid? excludeReservaId = null);
    }
}