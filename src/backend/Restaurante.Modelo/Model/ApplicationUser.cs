using Microsoft.AspNetCore.Identity;

namespace Restaurante.Modelo.Model
{
    // Custom user class for additional claims/properties
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        // Add more properties as needed
    }
}
