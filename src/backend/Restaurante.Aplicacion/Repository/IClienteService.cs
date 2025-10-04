using Microsoft.AspNetCore.JsonPatch;
using Restaurante.Modelo.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurante.Modelo.Model;

// IClienteService.cs in Restaurante.Aplicacion.Repository
namespace Restaurante.Aplicacion.Repository
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteDto>> GetAllAsync(int page, int pageSize, string? filter, string? sortBy);
        Task<ClienteDto?> GetByIdAsync(Guid id, bool includeReservas = false);
        Task<ClienteDto> CreateAsync(CreateClienteDto dto);
        Task UpdateAsync(Guid id, UpdateClienteDto dto);
        Task PatchAsync(Guid id, JsonPatchDocument<UpdateClienteDto> patch);
        Task DeleteAsync(Guid id);
    }
}