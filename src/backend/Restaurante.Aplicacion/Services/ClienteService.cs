using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Restaurante.Aplicacion.Repository;
using Restaurante.Infraestructura.Entities;
using Restaurante.Infraestructura.Repository;
using Restaurante.Modelo.Dto;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

// ClienteService.cs in Restaurante.Aplicacion.Services
namespace Restaurante.Aplicacion.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly IMapper _mapper;

        public ClienteService(IClienteRepository clienteRepository, IReservaRepository reservaRepository, IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _reservaRepository = reservaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ClienteDto>> GetAllAsync(int page, int pageSize, string? filter, string? sortBy)
        {
            Expression<Func<Cliente, bool>>? filterExpr = null;
            if (!string.IsNullOrEmpty(filter))
            {
                filterExpr = DynamicExpressionParser.ParseLambda<Cliente, bool>(new ParsingConfig(), true, "Email.Contains(@0) || Nombre.Contains(@0) || Apellidos.Contains(@0)", filter);
            }

            Func<IQueryable<Cliente>, IOrderedQueryable<Cliente>>? orderByFunc = null;
            if (!string.IsNullOrEmpty(sortBy))
            {
                orderByFunc = q => q.OrderBy(sortBy);
            }

            IEnumerable<Cliente> clientes = await _clienteRepository.GetAllAsync(
                filter: filterExpr,
                orderBy: orderByFunc,
                includeProperties: "Reservas",
                skip: (page - 1) * pageSize,
                take: pageSize);

            return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }

        public async Task<ClienteDto?> GetByIdAsync(Guid id, bool includeReservas = false)
        {
            var includeProps = includeReservas ? "Reservas" : null;
            var cliente = await _clienteRepository.GetByIdAsync(id, includeProps);
            return _mapper.Map<ClienteDto?>(cliente);
        }

        public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
        {
            if (await _clienteRepository.ExistsByEmailAsync(dto.Email))
            {
                throw new InvalidOperationException("A client with this email already exists.");
            }

            var cliente = _mapper.Map<Cliente>(dto);
            await _clienteRepository.AddAsync(cliente);
            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task UpdateAsync(Guid id, UpdateClienteDto dto)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new KeyNotFoundException($"Client with ID {id} not found.");
            }

            if (dto.Email != cliente.Email && await _clienteRepository.ExistsByEmailAsync(dto.Email))
            {
                throw new InvalidOperationException("A client with this email already exists.");
            }

            _mapper.Map(dto, cliente);
            await _clienteRepository.UpdateAsync(cliente);
        }

        public async Task PatchAsync(Guid id, JsonPatchDocument<UpdateClienteDto> patch)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new KeyNotFoundException($"Client with ID {id} not found.");
            }

            var dto = _mapper.Map<UpdateClienteDto>(cliente);
            patch.ApplyTo(dto);

            if (dto.Email != cliente.Email && await _clienteRepository.ExistsByEmailAsync(dto.Email))
            {
                throw new InvalidOperationException("A client with this email already exists.");
            }

            _mapper.Map(dto, cliente);
            await _clienteRepository.UpdateAsync(cliente);
        }

        public async Task DeleteAsync(Guid id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                throw new KeyNotFoundException($"Client with ID {id} not found.");
            }

            var activeReservas = await _reservaRepository.GetAllAsync(filter: r => r.ClienteId == id && r.Estado != "Cancelada" && r.Estado != "Completada");
            foreach (var r in activeReservas)
            {
                r.Estado = "Cancelada";
                r.FechaCancelacion = DateTime.UtcNow;
                await _reservaRepository.UpdateAsync(r);
            }

            await _clienteRepository.DeleteAsync(cliente);
        }
    }
}