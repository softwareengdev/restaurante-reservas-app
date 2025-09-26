using Restaurante.Modelo.Model.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurante.Aplicacion.Repository
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginRequest request);
        Task<TokenResponse> RegisterAsync(RegisterRequest request);
        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<TokenResponse> RevokeRefreshTokenAsync(string refreshToken);
    }
}
