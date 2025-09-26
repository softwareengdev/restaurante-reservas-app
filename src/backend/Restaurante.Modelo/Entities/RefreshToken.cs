using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurante.Modelo.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    }
}
