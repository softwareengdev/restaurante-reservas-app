using Restaurante.Modelo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaturante.Infraestructura.Repository
{
    public interface IRefreshTokenService
    {
        Task StoreRefreshTokenAsync(string userId, string token, DateTime expiry);
        Task<RefreshToken?> ValidateAndGetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
    }
}
