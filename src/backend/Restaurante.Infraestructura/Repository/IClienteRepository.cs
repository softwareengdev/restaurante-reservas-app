using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// For Cliente
namespace Restaurante.Infraestructura.Repository
{
    public interface IClienteRepository : IBaseRepository<Entities.Cliente>
    {
        Task<bool> ExistsByEmailAsync(string email);
    }
}