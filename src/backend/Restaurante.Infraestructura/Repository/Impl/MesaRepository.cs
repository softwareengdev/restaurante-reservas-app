using Microsoft.EntityFrameworkCore;
using Restaurante.Infraestructura.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Implementations in Restaurante.Infraestructura.Repository.Impl
// MesaRepository
namespace Restaurante.Infraestructura.Repository.Impl
{
    public class MesaRepository : BaseRepository<Entities.Mesa>, Repository.IMesaRepository
    {
        public MesaRepository(RestauranteDbContext context) : base(context) { }

        public async Task<bool> ExistsByNumeroAsync(string numero)
        {
            return await dbSet.AnyAsync(m => m.Numero == numero);
        }
    }
}