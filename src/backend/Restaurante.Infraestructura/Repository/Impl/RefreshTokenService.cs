using Microsoft.EntityFrameworkCore;
using Restaurante.Infraestructura.DBContext;
using Restaurante.Infraestructura.Repository;
using Restaurante.Modelo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurante.Infraestructura.Repository
{

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly RestauranteDbContext _context;

        public RefreshTokenService(RestauranteDbContext context)
        {
            _context = context;
        }

        public async Task StoreRefreshTokenAsync(string userId, string token, DateTime expiry)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiryDate = expiry,
                IsRevoked = false
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> ValidateAndGetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked && !rt.IsExpired);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
