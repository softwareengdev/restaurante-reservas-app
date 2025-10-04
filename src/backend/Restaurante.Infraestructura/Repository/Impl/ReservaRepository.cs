using Microsoft.EntityFrameworkCore;
using Restaurante.Infraestructura.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// ReservaRepository
namespace Restaurante.Infraestructura.Repository.Impl
{
    public class ReservaRepository : BaseRepository<Entities.Reserva>, Repository.IReservaRepository
    {
        public ReservaRepository(RestauranteDbContext context) : base(context) { }

        public async Task<bool> HasConflictAsync(Guid mesaId, DateTime start, DateTime end, Guid? excludeReservaId = null)
        {
            var query = dbSet.Where(r => r.MesaId == mesaId && r.Estado != "Cancelada" && r.FechaInicio < end && (r.FechaInicio + r.Duracion) > start);
            if (excludeReservaId.HasValue)
            {
                query = query.Where(r => r.Id != excludeReservaId.Value);
            }
            return await query.AnyAsync();
        }
    }
}