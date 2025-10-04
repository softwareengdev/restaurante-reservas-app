using Microsoft.AspNetCore.JsonPatch;
using Restaurante.Modelo.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurante.Modelo.Model;

// IReservaService.cs in Restaurante.Aplicacion.Repository
namespace Restaurante.Aplicacion.Repository
{
    public interface IReservaService
    {
        Task<IEnumerable<ReservaDto>> GetAllAsync(int page, int pageSize, string? filter, string? sortBy);
        Task<ReservaDto?> GetByIdAsync(Guid id);
        Task<ReservaDto> CreateAsync(CreateReservaDto dto);
        Task UpdateAsync(Guid id, UpdateReservaDto dto);
        Task PatchAsync(Guid id, JsonPatchDocument<UpdateReservaDto> patch);
        Task DeleteAsync(Guid id);
        Task CancelAsync(Guid id);
        Task<IEnumerable<MesaDto>> GetAvailableTablesAsync(int partySize, DateTime start, TimeSpan duration);
    }
}