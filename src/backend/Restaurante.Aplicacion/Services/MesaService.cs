using Microsoft.EntityFrameworkCore;
using Restaurante.Aplicacion.Repository;
using Restaurante.Infraestructura.Entities;
using Restaurante.Infraestructura.Repository;
using Restaurante.Modelo.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

// Implementations in Restaurante.Aplicacion.Services
// These will use the infra repositories for data access, and contain business logic
// Using AutoMapper as before

// MesaService
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Restaurante.Modelo.Model;
using System.Linq.Dynamic.Core;

namespace Restaurante.Aplicacion.Services
{
    public class MesaService : IMesaService
    {
        private readonly IMesaRepository _mesaRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly IMapper _mapper;

        public MesaService(IMesaRepository mesaRepository, IMapper mapper, IReservaRepository reservaRepository)
        {
            _mesaRepository = mesaRepository;
            _reservaRepository = reservaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MesaDto>> GetAllAsync(int page, int pageSize, string? filter, string? sortBy)
        {
            Expression<Func<Mesa, bool>>? filterExpr = null;
            if (!string.IsNullOrEmpty(filter))
            {
                filterExpr = DynamicExpressionParser.ParseLambda<Mesa, bool>(new ParsingConfig(), true, "Estado.Contains(@0) || Ubicacion.Contains(@0)", filter);
            }

            Func<IQueryable<Mesa>, IOrderedQueryable<Mesa>>? orderByFunc = null;
            if (!string.IsNullOrEmpty(sortBy))
            {
                orderByFunc = q => q.OrderBy(sortBy);
            }

            IEnumerable<Mesa> mesas = await _mesaRepository.GetAllAsync(
                filter: filterExpr,
                orderBy: orderByFunc,
                includeProperties: "Reservas",
                skip: (page - 1) * pageSize,
                take: pageSize);

            return _mapper.Map<IEnumerable<MesaDto>>(mesas);
        }

        public async Task<MesaDto?> GetByIdAsync(Guid id, bool includeReservas = false)
        {
            var includeProps = includeReservas ? "Reservas" : null;
            var mesa = await _mesaRepository.GetByIdAsync(id, includeProps);
            return _mapper.Map<MesaDto?>(mesa);
        }

        public async Task<MesaDto> CreateAsync(CreateMesaDto dto)
        {
            if (await _mesaRepository.ExistsByNumeroAsync(dto.Numero))
            {
                throw new InvalidOperationException("A table with this number already exists.");
            }

            var mesa = _mapper.Map<Mesa>(dto);
            await _mesaRepository.AddAsync(mesa);
            return _mapper.Map<MesaDto>(mesa);
        }

        public async Task UpdateAsync(Guid id, UpdateMesaDto dto)
        {
            var mesa = await _mesaRepository.GetByIdAsync(id);
            if (mesa == null)
            {
                throw new KeyNotFoundException($"Table with ID {id} not found.");
            }

            if (dto.Numero != mesa.Numero && await _mesaRepository.ExistsByNumeroAsync(dto.Numero))
            {
                throw new InvalidOperationException("A table with this number already exists.");
            }

            _mapper.Map(dto, mesa);
            await _mesaRepository.UpdateAsync(mesa);
        }

        public async Task PatchAsync(Guid id, JsonPatchDocument<UpdateMesaDto> patch)
        {
            var mesa = await _mesaRepository.GetByIdAsync(id);
            if (mesa == null)
            {
                throw new KeyNotFoundException($"Table with ID {id} not found.");
            }

            var dto = _mapper.Map<UpdateMesaDto>(mesa);
            patch.ApplyTo(dto);

            if (dto.Numero != mesa.Numero && await _mesaRepository.ExistsByNumeroAsync(dto.Numero))
            {
                throw new InvalidOperationException("A table with this number already exists.");
            }

            _mapper.Map(dto, mesa);
            await _mesaRepository.UpdateAsync(mesa);
        }

        public async Task DeleteAsync(Guid id)
        {
            var mesa = await _mesaRepository.GetByIdAsync(id);
            if (mesa == null)
            {
                throw new KeyNotFoundException($"Table with ID {id} not found.");
            }

            var activeCount = await _reservaRepository.CountAsync(r => r.MesaId == id && r.Estado != "Cancelada" && r.Estado != "Completada");
            if (activeCount > 0)
            {
                throw new InvalidOperationException("Cannot delete table with active reservations.");
            }

            await _mesaRepository.DeleteAsync(mesa);
        }
    }
}