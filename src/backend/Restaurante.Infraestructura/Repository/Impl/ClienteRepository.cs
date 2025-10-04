using Microsoft.EntityFrameworkCore;
using Restaurante.Infraestructura.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// ClienteRepository
namespace Restaurante.Infraestructura.Repository.Impl
{
    public class ClienteRepository : BaseRepository<Entities.Cliente>, Repository.IClienteRepository
    {
        public ClienteRepository(RestauranteDbContext context) : base(context) { }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await dbSet.AnyAsync(c => c.Email == email);
        }
    }
}