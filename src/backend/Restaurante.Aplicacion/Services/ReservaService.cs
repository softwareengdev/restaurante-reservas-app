using AutoMapper;
using Restaurante.Aplicacion.Repository;
using Restaurante.Infraestructura.Entities;
using Restaurante.Infraestructura.Repository;
using Restaurante.Modelo.Dto;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

// ReservaService.cs in Restaurante.Aplicacion.Services
namespace Restaurante.Aplicacion.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public ReservaService(IReservaRepository reservaRepository, IMesaRepository mesaRepository, IClienteRepository clienteRepository, IMapper mapper)
        {
            _reservaRepository = reservaRepository;
            _mesaRepository = mesaRepository;
            _clienteRepository = clienteRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReservaDto>> GetAllAsync(int page, int pageSize, string? filter, string? sortBy)
        {
            Expression<Func<Reserva, bool>>? filterExpr = null;
            if (!string.IsNullOrEmpty(filter))
            {
                DateTime fechaFilter;
                if (DateTime.TryParse(filter, out fechaFilter))
                {
                    filterExpr = r => r.FechaInicio.Date == fechaFilter.Date;
                }
                else
                {
                    filterExpr = DynamicExpressionParser.ParseLambda<Reserva, bool>(new ParsingConfig(), true, "Estado.Contains(@0)", filter);
                }
            }

            Func<IQueryable<Reserva>, IOrderedQueryable<Reserva>>? orderByFunc = null;
            if (!string.IsNullOrEmpty(sortBy))
            {
                orderByFunc = q => q.OrderBy(sortBy);
            }

            IEnumerable<Reserva> reservas = await _reservaRepository.GetAllAsync(
                filter: filterExpr,
                orderBy: orderByFunc,
                skip: (page - 1) * pageSize,
                take: pageSize);

            return _mapper.Map<IEnumerable<ReservaDto>>(reservas);
        }

        public async Task<ReservaDto?> GetByIdAsync(Guid id)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id, "Mesa,Cliente");
            return _mapper.Map<ReservaDto?>(reserva);
        }

        public async Task<ReservaDto> CreateAsync(CreateReservaDto dto)
        {
            var mesa = await _mesaRepository.GetByIdAsync(dto.MesaId);
            if (mesa == null)
            {
                throw new KeyNotFoundException($"Table with ID {dto.MesaId} not found.");
            }

            if (mesa.Capacidad < dto.NumeroPersonas)
            {
                throw new InvalidOperationException("Reservation exceeds table capacity.");
            }

            var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
            if (cliente == null)
            {
                throw new KeyNotFoundException($"Client with ID {dto.ClienteId} not found.");
            }

            var newEnd = dto.FechaInicio + dto.Duracion;
            if (await _reservaRepository.HasConflictAsync(dto.MesaId, dto.FechaInicio, newEnd))
            {
                throw new InvalidOperationException("Time slot conflicts with an existing reservation.");
            }

            if (dto.Duracion <= TimeSpan.Zero)
            {
                throw new InvalidOperationException("Duration must be positive.");
            }
            if (dto.FechaInicio < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Reservation start time must be in the future.");
            }

            var reserva = _mapper.Map<Reserva>(dto);
            reserva.Estado = "Pendiente";
            await _reservaRepository.AddAsync(reserva);

            cliente.NumeroVisitas++;
            cliente.PuntosLealtad += 10;
            await _clienteRepository.UpdateAsync(cliente);

            return _mapper.Map<ReservaDto>(reserva);
        }

        public async Task UpdateAsync(Guid id, UpdateReservaDto dto)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id);
            if (reserva == null)
            {
                throw new KeyNotFoundException($"Reservation with ID {id} not found.");
            }

            if (dto.FechaInicio != reserva.FechaInicio || dto.Duracion != reserva.Duracion || dto.MesaId != reserva.MesaId || dto.NumeroPersonas != reserva.NumeroPersonas)
            {
                var mesa = dto.MesaId != reserva.MesaId ? await _mesaRepository.GetByIdAsync(dto.MesaId) : reserva.Mesa;
                if (mesa == null)
                {
                    throw new KeyNotFoundException("Invalid table ID.");
                }
                if (mesa.Capacidad < dto.NumeroPersonas)
                {
                    throw new InvalidOperationException("Exceeds table capacity.");
                }

                var newEnd = dto.FechaInicio + dto.Duracion;
                if (await _reservaRepository.HasConflictAsync(dto.MesaId, dto.FechaInicio, newEnd, id))
                {
                    throw new InvalidOperationException("Updated time slot conflicts.");
                }
            }

            _mapper.Map(dto, reserva);
            await _reservaRepository.UpdateAsync(reserva);
        }

        public async Task PatchAsync(Guid id, JsonPatchDocument<UpdateReservaDto> patch)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id, "Mesa");
            if (reserva == null)
            {
                throw new KeyNotFoundException($"Reservation with ID {id} not found.");
            }

            var dto = _mapper.Map<UpdateReservaDto>(reserva);
            patch.ApplyTo(dto);

            if (dto.FechaInicio != reserva.FechaInicio || dto.Duracion != reserva.Duracion || dto.MesaId != reserva.MesaId || dto.NumeroPersonas != reserva.NumeroPersonas)
            {
                var mesa = dto.MesaId != reserva.MesaId ? await _mesaRepository.GetByIdAsync(dto.MesaId) : reserva.Mesa;
                if (mesa == null)
                {
                    throw new KeyNotFoundException("Invalid table ID.");
                }
                if (mesa.Capacidad < dto.NumeroPersonas)
                {
                    throw new InvalidOperationException("Exceeds table capacity.");
                }

                var newEnd = dto.FechaInicio + dto.Duracion;
                if (await _reservaRepository.HasConflictAsync(dto.MesaId, dto.FechaInicio, newEnd, id))
                {
                    throw new InvalidOperationException("Updated time slot conflicts.");
                }
            }

            _mapper.Map(dto, reserva);
            await _reservaRepository.UpdateAsync(reserva);
        }

        public async Task DeleteAsync(Guid id)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id);
            if (reserva == null)
            {
                throw new KeyNotFoundException($"Reservation with ID {id} not found.");
            }

            await _reservaRepository.DeleteAsync(reserva);
        }

        public async Task CancelAsync(Guid id)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id);
            if (reserva == null)
            {
                throw new KeyNotFoundException($"Reservation with ID {id} not found.");
            }

            if (reserva.Estado == "Cancelada")
            {
                throw new InvalidOperationException("Reservation already cancelled.");
            }

            reserva.Estado = "Cancelada";
            reserva.FechaCancelacion = DateTime.UtcNow;
            await _reservaRepository.UpdateAsync(reserva);

            var cliente = await _clienteRepository.GetByIdAsync(reserva.ClienteId);
            if (cliente != null)
            {
                cliente.PuntosLealtad -= 5;
                await _clienteRepository.UpdateAsync(cliente);
            }
        }

        public async Task<IEnumerable<MesaDto>> GetAvailableTablesAsync(int partySize, DateTime start, TimeSpan duration)
        {
            if (partySize < 1)
            {
                throw new ArgumentException("Party size must be at least 1.");
            }
            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentException("Duration must be positive.");
            }
            if (start < DateTime.UtcNow)
            {
                throw new ArgumentException("Start time must be in the future.");
            }

            var end = start + duration;

            var mesas = await _mesaRepository.GetAllAsync(
                filter: m => m.Capacidad >= partySize && m.Estado == "Disponible",
                orderBy: q => q.OrderBy(m => m.Capacidad));

            var available = new List<Mesa>();
            foreach (var m in mesas)
            {
                if (!await _reservaRepository.HasConflictAsync(m.Id, start, end))
                {
                    available.Add(m);
                }
            }

            return _mapper.Map<IEnumerable<MesaDto>>(available);
        }
    }
}