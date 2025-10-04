using Microsoft.AspNetCore.JsonPatch;
using Restaurante.Modelo.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// IMesaService.cs in Restaurante.Aplicacion.Repository
using Microsoft.AspNetCore.JsonPatch;
using Restaurante.Modelo.Model;

namespace Restaurante.Aplicacion.Repository
{
    public interface IMesaService
    {
        Task<IEnumerable<MesaDto>> GetAllAsync(int page, int pageSize, string? filter, string? sortBy);
        Task<MesaDto?> GetByIdAsync(Guid id, bool includeReservas = false);
        Task<MesaDto> CreateAsync(CreateMesaDto dto);
        Task UpdateAsync(Guid id, UpdateMesaDto dto);
        Task PatchAsync(Guid id, JsonPatchDocument<UpdateMesaDto> patch);
        Task DeleteAsync(Guid id);
    }
}